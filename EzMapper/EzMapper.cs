using EzMapper.Attributes;
using EzMapper.Database;
using EzMapper.Models;
using EzMapper.Reflection;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using EzMapper.Expressions;
using log4net;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace EzMapper
{
    public class EzMapper
    {

        private static readonly List<Type> _types = new();
        private static readonly ICache _cache = new MemoryCache();
        private static readonly ILog _logger = Default.GetLogger();

        private static IDatebase _db;
        private static bool _isBuilt = false;
        

        static EzMapper()
        {
            log4net.Config.XmlConfigurator.Configure();
        }
        private EzMapper() { }
        public static void Register<T>() where T : class
        {
            Assert.That(!_isBuilt, "The database has allready benn built. You can't register more types");

            _types.Add(typeof(T));
        }

        public static void Register(params Type[] types)
        {
            Assert.That(!_isBuilt, "The database has allready benn built. You can't register more types");
            Assert.That(types.All(t => t is not null), "type can not be null");

            _types.AddRange(types);
        }

        public static void RegisterTypesFromAssembly(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(type => type.GetInterfaces().Contains(typeof(IEzModel)));
            Assert.That(types.Any(), $"No models found in assembly {assembly.FullName} that implements {nameof(IEzModel)}");

            _types.AddRange(types);
        }

        public static void Build()
        {
            
            if (_isBuilt) return;

            _db = new Database<SQLiteConnection, SQLiteCommand, SQLiteParameter>($"Data Source=./{Default.DbName}; Foreign Keys=True; Version=3;");

                //add parent types to list to enable polymorphic reads
            repeat:
            foreach (Type type in _types)
            {
                if (type.BaseType != typeof(object) && !_types.Contains(type.BaseType))
                {
                    _types.Add(type.BaseType);
                    goto repeat; // repeat the loop to include the newly added type in the iteration
                }
            }


            if (File.Exists(Default.DbName)) goto end;

            SQLiteConnection.CreateFile(Default.DbName);
            List<Table> tables = new();

            _types.ForEach(type => tables.AddRange(ModelParser.CreateTables(type)));
            tables = tables.GroupBy(t => t.Name).Select(g => g.First()).ToList(); // get rid of duplicate tables

            foreach (string statement in SqlStatementBuilder.CreateCreateStatements(tables))
            {
                _logger.Debug(statement);
                _db.ExecuteNonQuery(statement);
            }

            foreach(var table in tables)
            {
                if (table.Triggers is null) continue;

                foreach(string triggerSql in table.Triggers)
                {
                    if(!string.IsNullOrEmpty(triggerSql))
                    {
                        _logger.Debug(triggerSql);
                        _db.ExecuteNonQuery(triggerSql);
                    }
                        
                }
            }

            end:
            _isBuilt = true;
        }


        public static async Task<int> DeleteAsync<T>(int id)
        {
            return await Task.Run(() => Delete<T>(id));
        }

        public static async Task<int> DeleteAsync(params object[] models)
        {
            return await Task.Run(() => Delete(models));
        }

        public static int Delete<T>(int id)
        {
            Assert.That(_isBuilt, "You can't delete objects yet, the database has not been built");
            Assert.That(_types.Contains(typeof(T)), $"object of type {typeof(T)} is not registered");

            //in case of inheritance, delete parent
            var tables = ModelParser.CreateTables(typeof(T));
            var table = tables.Where(t => t.Name == typeof(T).Name).First();

            Column pk = table.Columns.Where(col => col.IsPrimaryKey).First();

            //this will find the the root parent in case of inheritance
            while (pk.IsForeignKey)
            {
                var fk = table.ForeignKeys.Where(fk => fk.FieldName == pk.Name).First();
                var targetTable = tables.Where(t => t.Name == fk.TargetTable).First();

                table = targetTable;
                pk = targetTable.Columns.Where(col => col.IsPrimaryKey).First();
            }

            DeleteStatement stmt = new() { Table = table, ID = id };
            string sql = SqlStatementBuilder.CreateDeleteStatement(stmt);
            _logger.Debug(sql);

            try
            {
                int affectedRows = _db.ExecuteNonQuery(sql, _db.Param("p0", id));

                if (affectedRows == 1)
                    _cache.Delete<T>(id);

                return affectedRows;

            }
            catch(Exception ex)
            {
                throw new Exception($"error occured while deleting {typeof(T)} id: {id}. See inner exception for details", ex);
            }
        }

        public static int Delete(params object[] models)
        {
            Assert.That(_isBuilt, "You can't delete objects yet, the database has not been built");

            int count = 0;
            foreach (object model in models)
            {
                Assert.NotNull(model);
                Assert.That(_types.Contains(model.GetType()), $"object of type {model.GetType()} is not registered");

                try
                {
                    int pkValue = ModelParser.GetModelId(model);
                    count += (int)Types.InvokeGenericMethod(typeof(EzMapper), null, nameof(EzMapper.Delete), model.GetType(), pkValue);
                }
                catch(Exception ex)
                {
                    throw new Exception($"error occured while deleting {model.GetType().Name}. See inner exception for details", ex);
                }

            }

            return count;
        }

        public static async Task<T> GetAsync<T>(int id)
        {
            return await Task.Run(() => Get<T>(id));
        }
        public static T Get<T>(int id)
        {
            Assert.That(_isBuilt, "You can't retrieve objects yet, the database has not been built");
            Assert.That(_types.Contains(typeof(T)), $"object of type {typeof(T)} is not registered");

            var subTypes = Types.GetSubTypes<T>(_types.ToArray());

            try
            {
                //polymorphic read
                if (IsPolymorphicRead<T>())
                    return PolymorphicGet<T>(nameof(EzMapper.Get), id).FirstOrDefault();

                if (_cache.Contains<T>(id))
                    return _cache.Get<T>(id);

                string pkFieldName = ModelParser.GetPkFieldName(typeof(T));
                return RecursiveGet<T>(new WhereClause(pkFieldName, "=", id.ToString())).FirstOrDefault();
            }
            catch(Exception ex)
            {
                throw new Exception($"error occured while retrieving {typeof(T)} id: {id}. See inner exception for details", ex);
            }

        }

        public static async Task<IEnumerable<T>> GetAsync<T>()
        {
            return await Task.Run(() => Get<T>());
        }

        private static bool IsPolymorphicRead<T>()
        {
            var subTypes = Types.GetSubTypes<T>(_types.ToArray());
            return subTypes.Any();
        }

        private static IEnumerable<T> PolymorphicGet<T>(string callerName, params object[] args)
        {
            var subTypes = Types.GetSubTypes<T>(_types.ToArray());
            var result = new List<T>();

            foreach (Type subType in subTypes)
            {
                object returnedValue = Types.InvokeGenericMethod(typeof(EzMapper), null, callerName, subType, args);

                if(returnedValue is not null)
                {
                    if (returnedValue.GetType().IsAssignableTo(typeof(T)))
                        result.Add((T)returnedValue);
                    else
                        result.AddRange((IEnumerable<T>)returnedValue);
                }

            }

            return result;
        }

        public static IEnumerable<T> Get<T>()
        {
            Assert.That(_isBuilt, "You can't retrieve objects yet, the database has not been built");
            Assert.That(_types.Contains(typeof(T)), $"object of type {typeof(T)} is not registered");

            try
            {
                //polymorphic read
                if (IsPolymorphicRead<T>())
                    return PolymorphicGet<T>(nameof(EzMapper.Get));

                if (_cache.ContainsType<T>())
                    return _cache.Get<T>();

                IEnumerable<T> results = RecursiveGet<T>();
                AddResultsToCache(results);

                return results;
            }
             catch (Exception ex)
            {
                throw new Exception($"error occured while retrieving {typeof(T)}. See inner exception for details", ex);
            }
        }

        private static void AddResultsToCache<T>(IEnumerable<T> results)
        {
            foreach (T item in results)
            {
                int id = ModelParser.GetModelId(item);
                if (!_cache.Contains<T>(id))
                    _cache.Add(id, item);
            }
        }

        public static async Task<IEnumerable<T>> QueryAsync<T>(Expression<Func<T, bool>> expression)
        {
            return await Task.Run(() => Query(expression));
        }
        public static IEnumerable<T> Query<T>(Expression<Func<T, bool>> expression)
        {
            Assert.That(_isBuilt, "You can't retrieve objects yet, the database has not been built");
            Assert.That(_types.Contains(typeof(T)), $"object of type {typeof(T)} is not registered");

            var stmt = StatementBuilder.CreateSingleSelectStatement<T>();
            string sqlStatement = SqlStatementBuilder.CreateSelectStatement(stmt);

            //TODO: use parameters
            try
            {
                sqlStatement += $"WHERE {ExpressionParser.ParseExpression(expression.Body)}";
            }
            catch(Exception ex)
            {
                throw new Exception("something went wrong parsing the expression" + ex.Message);
            }

            _logger.Debug(sqlStatement);
            IEnumerable<T> results = _db.ExecuteQuery(sqlStatement, ObjectReader<T>);
            AddResultsToCache(results);

            return results;
        }

        private static IEnumerable<T> RecursiveGet<T>(WhereClause whereClause = null)
        {
            var stmt = StatementBuilder.CreateSingleSelectStatement<T>();
            string sqlStatement = SqlStatementBuilder.CreateSelectStatement(stmt, whereClause);
            _logger.Debug(sqlStatement);

            return _db.ExecuteQuery<T>(sqlStatement, ObjectReader<T>);
        }
        private static T ObjectReader<T>(DbDataReader reader)
        {
            T model = Activator.CreateInstance<T>();
            var props = model.GetType().GetProperties();

            foreach (var prop in props)
            {
                object value = null;

                if (Types.IsPrimitive(prop.PropertyType)) // read all primitive values (including inherited ones)
                {
                    // get primitive values
                    string columnName = $"{prop.DeclaringType.Name}_{prop.Name}";
                    int ordinal = reader.GetOrdinal(columnName);
                    value = reader.GetValue(ordinal);

                    if (prop.PropertyType == typeof(bool))
                        value = SQLiteConvert.ToBoolean(value);
                    else
                        value = Convert.ChangeType(value, prop.PropertyType);

                }
                else if (!Types.IsPrimitive(prop.PropertyType) && !Types.IsCollection(prop.PropertyType)) // read nested objects (1:1)
                {
                    // when the property is another object, we save a foreign key in the main table
                    string fkPropertyName = $"{prop.Name}{Default.IdProprtyName}";
                    string columnName = $"{prop.DeclaringType.Name}_{fkPropertyName}";
                    int ordinal = reader.GetOrdinal(columnName);
                    object fkValue = reader.GetValue(ordinal);
                    if (fkValue == DBNull.Value) continue;

                    fkValue = Convert.ChangeType(fkValue, typeof(int));

                    string targetPkPropertyName = ModelParser.GetPrimaryKeyPropertyName(prop.PropertyType);
                    string pk = $"{prop.PropertyType.Name}_{targetPkPropertyName}";

                    var where = new WhereClause(pk, "=", fkValue.ToString());

                    value = ((IEnumerable<object>)Types.InvokeGenericMethod(typeof(EzMapper), null, nameof(EzMapper.RecursiveGet), prop.PropertyType, where)).FirstOrDefault();
                }
                else if(Types.IsCollection(prop.PropertyType)) // collections
                {
                    Type elementType = Types.GetElementType(prop.PropertyType);

                    if ( Types.IsPrimitive( elementType) ) // 1:n of primitves
                    {
                        //a primitve table has 3 cols: pk, fk to owner and the value

                        //find table and foreign key names
                        string tableName = prop.DeclaringType.Name + prop.Name;
                        string fkName = prop.DeclaringType.Name + Default.IdProprtyName;

                        // get primary key value
                        string pkPropertyName = ModelParser.GetPrimaryKeyPropertyName(typeof(T));
                        string pkColumnName = $"{prop.DeclaringType.Name}_{pkPropertyName}";
                        int ordinal = reader.GetOrdinal(pkColumnName);

                        object pKvalue = Convert.ChangeType( reader.GetValue(ordinal), typeof(int) );
                        string rawSql = $"SELECT {prop.Name} FROM {tableName} WHERE {fkName} = @fkvalue";


                        //this will return IEnumerable<object>
                        object val = _db.ExecuteQuery(rawSql, (innerReader) => Convert.ChangeType(innerReader.GetValue(innerReader.GetOrdinal(prop.Name)), elementType), _db.Param("fkvalue", pKvalue));
                        value = CollectionsHelper.FillCollection((IEnumerable<object>)val, prop.PropertyType);
                    }
                    else
                    {
                        // find pk and pk value of owner
                        string pkPropertyName = ModelParser.GetPrimaryKeyPropertyName(elementType);

                        //find the pk property so we can find the declaring type of primary key (in case we are dealing with inheritance)
                        var pkProp = props.Where(p => p.Name == pkPropertyName).First();

                        string pkColumnName = $"{pkProp.DeclaringType.Name}_{pkPropertyName}";
                        int pkOrdinal = reader.GetOrdinal(pkColumnName);

                        object pKvalue = Convert.ChangeType(reader.GetValue(pkOrdinal), typeof(int));

                        if (Types.HasAttribute<SharedAttribute>(prop)) // m:n
                        {
                            // get the foriegn key names of the assignment table

                            string childTableName = elementType.Name;
                            string childTablePkName = ModelParser.GetPrimaryKeyPropertyName(elementType);
                            string assignmentTableName = model.GetType().Name + "_" + elementType.Name;
                            string fkToOwnerColumnName = model.GetType().Name + Default.IdProprtyName;
                            string fkToChildColumnName = elementType.Name + Default.IdProprtyName;

                            // select the ids of the child objects from the assignment table
                            var rawSql = $"SELECT {fkToChildColumnName} FROM {assignmentTableName} WHERE {fkToOwnerColumnName} = @fkvalue";
                            IEnumerable<int> ids = _db.ExecuteQuery(rawSql, innerReader => Convert.ToInt32(innerReader.GetValue(0)), _db.Param("fkvalue", pKvalue.ToString()));

                            var values = ids.Select(id =>
                            {
                                var where = new WhereClause(childTablePkName, "=", id.ToString());
                                object val = Types.InvokeGenericMethod(typeof(EzMapper), null, nameof(EzMapper.RecursiveGet), elementType, where);
                                return ((IEnumerable<object>)val).FirstOrDefault();
                            });

                            value = CollectionsHelper.FillCollection(values, prop.PropertyType);
                        }
                        else // 1:n complex
                        {

                            //find fk name in child table
                            string fkColumnName = $"{prop.DeclaringType.Name}{Default.IdProprtyName}";

                            var where = new WhereClause(fkColumnName, "=", pKvalue.ToString());

                            value = (IEnumerable<object>)Types.InvokeGenericMethod(typeof(EzMapper), null, nameof(EzMapper.RecursiveGet), elementType, where); 
                        }
                    }
                }


                prop.SetValue(model, value);
            }

            return model;
        }

        public static async Task SaveAsync(params object[] models)
        {
            await Task.Run(() => Save(models));
        }

        public static void Save(params object[] models)
        {
            Assert.That(_isBuilt, "You can't save objects yet, the database has not been built");

            foreach(object model in models)
            {
                Assert.NotNull(model, nameof(model));
                Assert.That(_types.Contains(model.GetType()), $"object of type {model} is not registered");

                List<Table> tables = ModelParser.CreateTables(model.GetType()).GroupBy(t => t.Name).Select(g => g.First()).ToList();
                List<Table> sortedTables = StatementBuilder.SortTablesByForeignKeys(tables).ToList();
                List<InsertStatement> insertStatements = new();

                sortedTables.ForEach(table => 
                {
                    var statements = StatementBuilder.TableToInsertStatements(table, model, sortedTables.ToArray());
                    if(statements is not null) insertStatements.AddRange(statements); 
                });

                //insert statements in assignment tables should always go last
                var assignmentInserts = insertStatements.Where(ins => ins.Table.Type == typeof(ManyToManyAssignmentTable)).ToList();
                insertStatements.RemoveAll(ins => ins.Table.Type == typeof(ManyToManyAssignmentTable));
                insertStatements.AddRange(assignmentInserts);

                foreach (InsertStatement stmt in insertStatements)
                {
                    IEnumerable<DbParameter> paras = ModelParser.GetDbParams(stmt, sortedTables, insertStatements, _db);
                    string sql = SqlStatementBuilder.CreateInsertStatement(stmt);
                    _logger.Debug(sql);

                    try
                    {
                        _db.ExecuteNonQuery(sql, paras.ToArray());
                    }
                    catch(Exception ex)
                    {
                        throw new Exception($"error saving {model.GetType().Name}. see inner exception for details", ex);
                    }

                    sortedTables.ForEach(t => t.Columns.ForEach(c => c.Ignored = false)); // QUICK FIX:
                    // PROBLEM: the method GetDbParams will set the IsIgnored proprty of a table if the value of a foreign key is null
                    // but all tables all shared, so the next record of the same table will have the IsIgnored flag set no matter what value the fk has
                    //quick fix: reset the state of all tables after an excution of an insert
                }


                int modelId = ModelParser.GetModelId(model);

                if(!(bool)Types.InvokeGenericMethod(typeof(ICache), _cache, nameof(_cache.Contains), model.GetType(), modelId))
                {
                    var addMethod = typeof(ICache) // work around
                            .GetMethods()
                            .Single(m => m.Name == nameof(ICache.Add) && m.IsGenericMethodDefinition);

                    var genericAddMethod = addMethod.MakeGenericMethod(model.GetType());

                    genericAddMethod.Invoke(_cache, new[] { modelId, model });
                }
            }
        }

        public static async Task UpdateAsync(params object[] models)
        {
            await Task.Run(() => Update(models));
        }

        public static void Update(params object[] models)
        {
            Assert.That(_isBuilt, "You can't update objects yet, the database has not been built");

            foreach(object model in models)
            {
                Assert.NotNull(model);
                Assert.That(_types.Contains(model.GetType()), $"model of type {model.GetType()} is not registered");


                try
                {
                    int id = ModelParser.GetModelId(model);

                    Types.InvokeGenericMethod(typeof(EzMapper), null, nameof(EzMapper.Delete), model.GetType(), id);
                    Save(model);
                }
                catch (Exception ex)
                {
                    throw new Exception($"error updating {model.GetType().Name}. see inner exception for details", ex);
                }
            }
        }
    }
}
