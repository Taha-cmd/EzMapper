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

namespace EzMapper
{
    public class EzMapper
    {

        private static readonly List<Type> _types = new();
        private static bool _isBuilt = File.Exists(Default.DbName);
        private static IDatebase _db;

        static EzMapper()
        {
//#if DEBUG
//            File.Delete(Default.DbName);
//#endif
            _db = new Database<SQLiteConnection, SQLiteCommand, SQLiteParameter>($"Data Source=./{Default.DbName}");
        }
        private EzMapper() { }
        public static void Register<T>() where T : class
        {
            if (_isBuilt)
                throw new Exception("The database has allready benn built. You can't register more types");

            _types.Add(typeof(T));
        }

        public static void Register(params Type[] types)
        {
            if (_isBuilt)
                throw new Exception("The database has allready benn built. You can't register more types");

            Assertion.That(types.All(t => t is not null), "type can not be null");

            _types.AddRange(types);
        }

        public static void Build()
        {
            
            if (_isBuilt) return;

            SQLiteConnection.CreateFile(Default.DbName);

            _db = new Database<SQLiteConnection, SQLiteCommand, SQLiteParameter>($"Data Source=./{Default.DbName}");
            List<Table> tables = new();

            _types.ForEach(type => tables.AddRange(ModelParser.CreateTables(type)));
            tables = tables.GroupBy(t => t.Name).Select(g => g.First()).ToList(); // get rid of duplicate tables

            foreach (string statement in SqlStatementBuilder.CreateCreateStatements(tables))
            {
                _db.ExecuteNonQuery(statement);
            }

            _isBuilt = true;
        }

        public static T Get<T>(int id)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<T> Get<T>()
        {
            if (!_types.Contains(typeof(T)))
                throw new Exception($"object of type {typeof(T)} is not registered");

            return RecursiveGet<T>();

        }

        private static IEnumerable<T> RecursiveGet<T>(WhereClause whereClause = null)
        {
            var stmt = CreateSingleSelectStatement<T>();
            string sqlStatement = SqlStatementBuilder.CreateSelectStatement(stmt, whereClause);

            return _db.ExecuteQuery<T>(sqlStatement, ObjectReader<T>);

        }
        private static T ObjectReader<T>(DbDataReader reader)
        {
            T model = Activator.CreateInstance<T>();
            var props = model.GetType().GetProperties();

            foreach (var prop in props)
            {
                object value;

                if (Types.IsPrimitive(prop.PropertyType)) // read all primitive values (including inherited ones)
                {
                    // get primitive values
                    string columnName = $"{prop.DeclaringType.Name}_{prop.Name}";
                    int ordinal = reader.GetOrdinal(columnName);
                    value = reader.GetValue(ordinal);

                    value = Convert.ChangeType(value, prop.PropertyType);
                    prop.SetValue(model, value);
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

                    string targetPkPropertyName = ModelParser.GetPrimaryKeyPropertyName(prop.PropertyType.GetProperties());
                    string pk = $"{prop.PropertyType.Name}_{targetPkPropertyName}";

                    var where = new WhereClause(pk, "=", fkValue.ToString());

                    value = ((IEnumerable<object>)Types.InvokeGenericMethod(typeof(EzMapper), null, "RecursiveGet", prop.PropertyType, where)).FirstOrDefault();
                    prop.SetValue(model, value);
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
                        string pkPropertyName = ModelParser.GetPrimaryKeyPropertyName(typeof(T).GetProperties());
                        string pkColumnName = $"{prop.DeclaringType.Name}_{pkPropertyName}";
                        int ordinal = reader.GetOrdinal(pkColumnName);

                        object pKvalue = Convert.ChangeType( reader.GetValue(ordinal), typeof(int) );
                        string rawSql = $"SELECT {prop.Name} FROM {tableName} WHERE {fkName} = @fkvalue";

                        //TODO: refactor collection management code

                        //this will return IEnumerable<object>
                        value = _db.ExecuteQuery(rawSql, (innerReader) => Convert.ChangeType(innerReader.GetValue(innerReader.GetOrdinal(prop.Name)), elementType), _db.Param("fkvalue", pKvalue));

                        //IList is common interface for lists and arrays
                        IList values = (IList)value;
                        IList collectionValue = (IList)Activator.CreateInstance(prop.PropertyType, values.Count);

                        bool isList = collectionValue.GetType().GetMethod("Add") is not null; // array types have an Add method from the IList interface, which always throws and exception, but GetMethod retuns false. WTF

                        //https://docs.microsoft.com/en-us/dotnet/api/system.array?view=net-5.0
                        // calling Add method on an array as an IList always throws notsupportedexception
                        for (int i = 0; i < values.Count; i++)
                        {

                            if(isList)
                            {
                                collectionValue.Add(values[i]);
                                continue;
                            }
                            
                            //array
                            collectionValue[i] = values[i];
                        }

                        prop.SetValue(model, collectionValue);
                    }
                    else
                    {
                        // find pk and pk value of owner
                        string pkPropertyName = ModelParser.GetPrimaryKeyPropertyName(props);

                        //find the pk property so we can find thedeclaring type of primary key (in case we are dealing with inheritance)
                        var pkProp = props.Where(p => p.Name == pkPropertyName).First();

                        string pkColumnName = $"{pkProp.DeclaringType.Name}_{pkPropertyName}";
                        int pkOrdinal = reader.GetOrdinal(pkColumnName);

                        object pKvalue = Convert.ChangeType(reader.GetValue(pkOrdinal), typeof(int));

                        //TODO: and m:n
                        if (Types.HasAttribute<SharedAttribute>(prop)) // m:n
                        {
                            // get the foriegn key names of the assignment table

                            string childTableName = elementType.Name;
                            string childTablePkName = ModelParser.GetPrimaryKeyPropertyName(elementType.GetProperties());
                            string assignmentTableName = model.GetType().Name + "_" + elementType.Name;
                            string fkToOwnerColumnName = model.GetType().Name + Default.IdProprtyName;
                            string fkToChildColumnName = elementType.Name + Default.IdProprtyName;

                            // select the ids of the child objects from the assignment table
                            var rawSql = $"SELECT {fkToChildColumnName} FROM {assignmentTableName} WHERE {fkToOwnerColumnName} = @fkvalue";
                            IEnumerable<int> ids = _db.ExecuteQuery(rawSql, innerReader => Convert.ToInt32(innerReader.GetValue(0)), _db.Param("fkvalue", pKvalue.ToString()));

                            Type listType = typeof(List<>).MakeGenericType(elementType);
                            IList list = (IList)Activator.CreateInstance(listType);

                            foreach(int id in ids)
                            {
                                var where = new WhereClause(childTablePkName, "=", id.ToString());
                                value = Types.InvokeGenericMethod(typeof(EzMapper), null, "RecursiveGet", elementType, where);
                                list.Add(((IEnumerable<object>)value).FirstOrDefault());
                            }

                            if(prop.PropertyType.IsArray)
                            {
                                IList arr = (IList)Activator.CreateInstance(prop.PropertyType, list.Count);

                                for (int i = 0; i < ids.Count(); i++)
                                    arr[i] = list[i];

                                list = arr;
                            }

                            prop.SetValue(model, list);
                        }
                        else // 1:n complex
                        {

                            //find fk name in child table
                            string fkColumnName = $"{prop.DeclaringType.Name}{Default.IdProprtyName}";

                            var where = new WhereClause(fkColumnName, "=", pKvalue.ToString());

                            value = (IEnumerable<object>)Types.InvokeGenericMethod(typeof(EzMapper), null, "RecursiveGet", elementType, where);
                            prop.SetValue(model, value);
                        }
                    }
                }
            }

            return model;
        }

        //private static 

        private static SelectStatement CreateSingleSelectStatement<T>() // will return a select statement for the primitves of an object (includes a join to parents if inherited)
        {
            List<Table> tables = SortTablesByForeignKeys(ModelParser.CreateTables(typeof(T)).GroupBy(t => t.Name).Select(g => g.First())).ToList();
            Table mainTable = tables.Where(t => t.Type == typeof(T)).First();
            List<Join> allJoins = GetJoins(mainTable, tables, mainTable).ToList();
            List<Join> joins = new();

            Column pk = mainTable.Columns.Where(col => col.IsPrimaryKey).First();

            var stmt = new SelectStatement(mainTable);

            while(pk.IsForeignKey)
            {
                var fk = mainTable.ForeignKeys.Where(fk => fk.FieldName == pk.Name).First();
                var targetTable = tables.Where(t => t.Name == fk.TargetTable).First();
                joins.Add(allJoins.Where(j => j.Table.Name == mainTable.Name).First());

                mainTable = targetTable;
                pk = targetTable.Columns.Where(col => col.IsPrimaryKey).First();
            }

            stmt.Joins.AddRange(joins);

            return stmt;
        }

        private static IEnumerable<Join> GetJoins(Table mainTable, List<Table> tables, Table originalTable, bool ignoreInManyToManyAssignment = false)
        {

            //important: create an alias for the target table on each join to avoid ambiguity
            // do that by cloning the table, since we are dealing with reference types here, creating a new alias will affect all tables

            Assertion.NotNull(mainTable, nameof(mainTable));


            List<Table> tablesClone = new(tables);
            List<Join> joins = new();


            // this will handle cases where the foreign key is in the main table (1:1 and inheritance)
            foreach (var col in mainTable.Columns)
            {
                if(col.IsForeignKey)
                {
                    ForeignKey fk = mainTable.ForeignKeys.Where(f => f.FieldName == col.Name).First();
                    Table target = tablesClone.Where(t => t.Name == fk.TargetTable).First();

                    // only create a join if the maintable contains the target table (actual 1:1 relationship) or the target is a base class
                    // if the foreign key is the other way around, it is a 1:1 relationship
                    if (Types.HasObjectOfType(mainTable.Type, target.Type) || mainTable.Type.IsAssignableTo(target.Type)) 
                    {
                        // cloning the table will create a new alias, needed if multiple tables join on the same table
                        // example: select student, student has laptop and phone, each laptop and phone have a cpu. the problem will occur when we join on the cpu twice, thus we need a different alias
                        var targetTable = target.Clone(); 

                        var j = new Join() { Table = mainTable, ForeignKey = col.Name, TargetTable = targetTable, PrimaryKey = targetTable.PrimaryKey };
                        joins.Add(j);
                        joins.AddRange(GetJoins(targetTable, tablesClone, mainTable)); // this takes care of nested objects
                    }

                }
            }



            // this will handle cases where other tables contain a foreign key that points to the main table
            // (1:n and m:n)
            foreach (var table in tablesClone)
            {
                if (table.Name == mainTable.Name) continue; // skip checking relationships between the same table

                foreach(var col in table.Columns)
                {
                    if(col.IsForeignKey)
                    {
                        ForeignKey fk = table.ForeignKeys.Where(f => f.FieldName == col.Name).First();

                        if (fk.TargetTable == mainTable.Name) // if the foreign key points to the current table
                        {
                            if (table.Type == typeof(PrimitivesChildTable))
                            {
                                // this table has 3 cols: value col, pk and fk. fk points to main table's pk
                                // we perform the join from the main table to the collection's table, so we need to reverse the primary and foreign key
                                var targetTable = table.Clone();
                                joins.Add(new Join() { Table = mainTable, ForeignKey = mainTable.PrimaryKey, TargetTable = targetTable, PrimaryKey = fk.FieldName });
                            }
                            else if (table.Type == typeof(ManyToManyAssignmentTable))
                            {
                                if (ignoreInManyToManyAssignment) continue;

                                // join parent to assignment table
                                joins.Add(new Join() { Table = mainTable, ForeignKey = mainTable.PrimaryKey, TargetTable = table, PrimaryKey = fk.FieldName });

                                //find the table of the shared objects
                                string targetTableName = table.Name.Split("_")[1];
                                Table targetTable = tables.Where(t => t.Name == targetTableName).First();

                                // join assignment table to shared objects
                                joins.Add(new Join() { Table = table, PrimaryKey = targetTable.PrimaryKey, TargetTable = targetTable, ForeignKey =  targetTableName + Default.IdProprtyName });

                                //make a recursive call in case the shared object has dependencies
                                joins.AddRange(GetJoins(targetTable, tablesClone, targetTable, true));
                            }
                            else if(Types.HasCollectionOfType(mainTable.Type, table.Type))
                            {
                                joins.Add(new Join() { Table = mainTable, ForeignKey = mainTable.PrimaryKey, TargetTable = table, PrimaryKey = fk.FieldName });
                                joins.AddRange(GetJoins(table, tablesClone, mainTable)); // get the dependencies of the object  
                            }
                        }
                    }
                }
            }

            return joins;//.GroupBy(j => j.TargetTable).Select(g => g.First());
        }

        public static async Task SaveAsync(params object[] models)
        {
            await Task.Run(() => Save(models));
        }

        public static void Save(params object[] models)
        {
            if (!_isBuilt)
                throw new Exception("You can't save objects yet, the database has not been built");

            foreach(object model in models)
            {
                Assertion.NotNull(model, nameof(model));

                if (!_types.Contains(model.GetType()))
                    throw new Exception($"object of type {model} is not registered");

                List<Table> tables = SortTablesByForeignKeys(ModelParser.CreateTables(model.GetType()).GroupBy(t => t.Name).Select(g => g.First())).ToList();
                List<InsertStatement> insertStatements = new();

                tables.ForEach(table => insertStatements.AddRange(StatementBuilder.TableToInsertStatements(table, model, tables.ToArray())));

                foreach (InsertStatement stmt in insertStatements)
                {
                    IEnumerable<DbParameter> paras = ModelParser.GetDbParams(stmt, tables, insertStatements, _db);
                    string sql = SqlStatementBuilder.CreateInsertStatement(stmt);

                    _db.ExecuteNonQuery(sql, paras.ToArray());
                }
            }
        }

        private static IEnumerable<Table> SortTablesByForeignKeys(IEnumerable<Table> tables)
        {
            List<Table> results = new();

            foreach (Table table in tables)
            {
                foreach (Column column in table.Columns)
                {
                    if (column.IsForeignKey)
                        results.AddRange(SortTablesByForeignKeys(tables.Where(t => t.Name == table.ForeignKeys.Where(fk => fk.FieldName == column.Name).First().TargetTable)));
                }

                results.Add(table);
            }

            return results.GroupBy(t => t.Name).Select(g => g.First());

        }

    }

    
}
