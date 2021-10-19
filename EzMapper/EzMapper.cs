using EzMapper.Attributes;
using EzMapper.Database;
using EzMapper.Models;
using EzMapper.Reflection;
using System;
using System.Collections.Generic;
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
#if DEBUG
            File.Delete(Default.DbName);
#endif
            _db = new Database<SQLiteConnection, SQLiteCommand, SQLiteParameter>($"Data Source=./{Default.DbName}");
        }
        private EzMapper() { }
        public static void Register<T>() where T : class
        {
            if (_isBuilt)
                throw new Exception("The database has allready benn built. You can't register more types");

            _types.Add(typeof(T));
        }

        public static void Build()
        {
            
            if (_isBuilt) return;

            SQLiteConnection.CreateFile(Default.DbName);

            _db = new Database<SQLiteConnection, SQLiteCommand, SQLiteParameter>($"Data Source=./{Default.DbName}");
            List<Table> tables = new();

            _types.ForEach(type => tables.AddRange(CreateTables(type)));
            tables = tables.GroupBy(t => t.Name).Select(g => g.First()).ToList(); // get rid of duplicate tables

            foreach (string statement in SqlStatementBuilder.CreateCreateStatements(tables))
            {
                _db.ExecuteNonQuery(statement);
            }

            _isBuilt = true;
        }


        // not needed?

        //private static IEnumerable<Table> SortTablesForCreation(IEnumerable<Table> tables)
        //{
        //    // in case the user registers a model that is contained in a list within another model,
        //    // then this model will be created twice (the second time being when the parent model is registered)
        //    // and the foreign key will not be set correctly
        //    // this is why we group tables by name (models that were registered more than once)
        //    // and take the one with the most columns (the correct one with the foreign key)
        //    tables = tables
        //                .GroupBy(table => table.Name)
        //                .Select(g => g.OrderByDescending(table => table.Columns.Count))
        //                .Select(g => g.First())
        //                .OrderByDescending(table => table.ForeignKeys.Count == 0)
        //                .ToList(); //https://stackoverflow.com/questions/489258/linqs-distinct-on-a-particular-property


        //    // the last step will ruin the order and cause tables with foreign keys to come before their parent tables
        //    // take each table, and find all other tables that reference it (basically find all child tables to a parent table)
        //    // and append them to the list
        //    var tmp = new List<Table>();
        //    foreach (var table in tables)
        //    {
        //        tmp.AddRange(
        //            tables.Where(
        //                t => t.ForeignKeys.Where(fk => fk.TargetTable == table.Name).Count() > 0
        //            )
        //        );
        //    }

        //    tables = tables.Concat(tmp);

        //    // now we have duplicates, the tables at the end are the correct ones
        //    tables.Reverse();
        //    tables = tables.GroupBy(t => t.Name).Select(g => g.First()).ToList();
        //    tables.Reverse();

        //    return tables;
        //}

        public static void TestCrud(object model)
        {
            var props = model.GetType().GetProperties().ToList();
            //props.ForEach(prop => Console.WriteLine($"{prop.PropertyType} {prop.Name} {{ {prop.GetGetMethod()}; {prop.GetSetMethod()}; }}"));

            var select = new StringBuilder($"SELECT * FROM {model.GetType().Name};");
            var insret = new StringBuilder($"INSERT INTO {model.GetType().Name} (");
            

            props.ForEach(prop => insret.Append(prop.Name + ","));
            insret.Replace(",", "", insret.Length - 1, 1); // get rid of trailing comma
            insret.Append(") VALUES (");

            props.ForEach(prop => insret.Append($"@{prop.Name},"));
            insret.Replace(",", "", insret.Length - 1, 1); // get rid of trailing comma
            insret.Append(");");


            var update = new StringBuilder($"UPDATE {model.GetType().Name} SET ");
            props.Where(prop => prop.Name != "ID").ToList().ForEach(prop => update.Append($"{prop.Name}=@{prop.Name},"));
            update.Replace(",", "", update.Length - 1, 1); // get rid of trailing comma
            update.Append(" WHERE ID=@ID;");

            var delete = new StringBuilder($"DELETE FROM {model.GetType().Name} WHERE ID=@ID;");

            Console.WriteLine(select);
            Console.WriteLine(insret);
            Console.WriteLine(update);
            Console.WriteLine(delete);

        }

        public static T Get<T>(int id)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<T> Get<T>()
        {
            if (!_types.Contains(typeof(T)))
                throw new Exception($"object of type {typeof(T)} is not registered");

            
            List<Table> tables = SortTablesByForeignKeys(CreateTables(typeof(T)).GroupBy(t => t.Name).Select(g => g.First())).ToList();

            Table mainTable = tables.Where(t => t.Type == typeof(T)).First();
            List<Join> joins = GetJoins(mainTable, tables, mainTable).ToList();
            

            SelectStatement stmt = new(mainTable, joins.ToArray());
            string sqlStatement = SqlStatementBuilder.CreateSelectStatement(stmt);
            Console.WriteLine(sqlStatement);


            //HERE

            return new List<T>();
        }


        private static IEnumerable<Join> GetJoins(Table mainTable, List<Table> tables, Table originalTable)
        {
            Assertion.NotNull(mainTable, nameof(mainTable));


            List<Table> tablesClone = new(tables);
            List<Join> joins = new();


            //deal with the easy case: nested objects and inheritance
            foreach (var col in mainTable.Columns)
            {
                if(col.IsForeignKey)
                {
                    ForeignKey fk = mainTable.ForeignKeys.Where(f => f.FieldName == col.Name).First();
                    Table target = tablesClone.Where(t => t.Name == fk.TargetTable).First();
                    var j = new Join() { Table = mainTable, ForeignKey = col.Name, TargetTable = target, PrimaryKey = target.PrimaryKey };
                    joins.Add(j);
                    joins.AddRange(GetJoins(target, tablesClone, mainTable)); // this takes care of nested objects
                }
            }


            foreach (var table in tablesClone)
            {
                if (table.Type == typeof(PrimitivesChildTable))
                {
                    //find the owner's table (the property might be inherited)
                    //if the primary key is also a foreign key, then the target table is the owner of the collection

                    // this table has 3 cols: value col, pk and fk. fk points to main table's pk
                    var fk = table.ForeignKeys[0];
                    var target = tables.Where(t => t.Name == fk.TargetTable).First();

                    // we perform the join from the main table to the collection's table, so we need to reverse the primary and foreign key
                    joins.Add(new Join() { Table = originalTable, ForeignKey = originalTable.PrimaryKey, TargetTable = table, PrimaryKey = fk.FieldName });
                }
                else if(table.Type == typeof(ManyToManyAssignmentTable))
                {
                    //TODO: deal with m:n
                }
            }

            


            return joins.GroupBy(j => j.TargetTable).Select(g => g.First());
        }

        public static async Task SaveAsync(object model)
        {
            await Task.Run(() => Save(model));
        }

        public static void Save(object model)
        {
            Assertion.NotNull(model, nameof(model));

            if (!_isBuilt)
                throw new Exception("You can't save objects yet, the database has not been built");

            if (!_types.Contains(model.GetType()))
                throw new Exception($"object of type {model} is not registered");


            List<Table> tables = SortTablesByForeignKeys(CreateTables(model.GetType()).GroupBy(t => t.Name).Select(g => g.First())).ToList();
            List<InsertStatement> insertStatements = new();

            tables.ForEach(table => insertStatements.AddRange(StatementBuilder.TableToInsertStatements(table, model, tables.ToArray())));

            foreach (InsertStatement stmt in insertStatements)
            {
                IEnumerable<DbParameter> paras = ModelParser.GetDbParams(stmt, tables, insertStatements, _db);
                string sql = SqlStatementBuilder.CreateInsertStatement(stmt);

                _db.ExecuteNonQuery(sql, paras.ToArray());
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


        private static IEnumerable<Table> CreateTables(Type type)
        {
            return GetTableHierarchy(Activator.CreateInstance(type));
        }
        private static IEnumerable<Table> GetTableHierarchy(object model, List<Column> cols = null, List<ForeignKey> foreignKeys = null, string tableName = null, Type type = null)
        {
            List<Table> tables = new();
            string primaryKey = string.Empty;
            var props = model?.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).ToList();
            tableName ??= model.GetType().Name;

            var fks = foreignKeys is null ? new List<ForeignKey>() : foreignKeys;
            var columns = cols is null ? new List<Column>() : cols;
            
            if (model is null)
                goto TableBulding;

            type ??= model.GetType();

            if (Types.HasParentModel(model))
            {
                tables.AddRange(GetTableHierarchy(Activator.CreateInstance(model.GetType().BaseType)));
            }

            bool fk = false;
            if (!Types.HasParentModel(model))
            {
                primaryKey = ModelParser.GetPrimaryKeyPropertyName(props.ToArray());
            }
            else
            {
                primaryKey = model.GetType().BaseType.Name + Default.IdProprtyName;
                fks.Add(new ForeignKey(primaryKey, model.GetType().BaseType.Name, ModelParser.GetPrimaryKeyPropertyName(model.GetType().BaseType.GetProperties().ToArray())));
                fk = true;
            }

            columns.Add(new Column(primaryKey, "PRIMARY KEY") { IsForeignKey = fk});

            foreach(var prop in props?.Where(prop => prop.Name != primaryKey))
            {
                if (!Types.IsPrimitive(model, prop.Name))
                {
                    //a non primitive could be an object or a collection

                    if(Types.IsCollection(prop.PropertyType))
                    {
                        Type elementType = Types.GetElementType(prop.PropertyType);
                        var tableCols = new List<Column>();
                        var tableFks = new List<ForeignKey>();

                        if (Types.HasAttribute<SharedAttribute>(prop))
                        {
                            // m:n relathionship
                            // when we have m:n relationships, both types are non primitives

                            // create table for the new object
                            tables.AddRange(GetTableHierarchy(Activator.CreateInstance(elementType)));

                            // create assignment table (id, fk1, fk2)
                            tableCols.Add(new Column(Default.IdProprtyName, "PRIMARY KEY"));
                            tableCols.Add(new Column($"{tableName}{Default.IdProprtyName}", true));// reference parent table
                            tableCols.Add(new Column($"{elementType.Name}{Default.IdProprtyName}", true)); // reference child table

                            tableFks.Add(new ForeignKey($"{tableName}{Default.IdProprtyName}", model.GetType().Name, primaryKey));
                            tableFks.Add(new ForeignKey($"{elementType.Name}{Default.IdProprtyName}", elementType.Name, ModelParser.GetPrimaryKeyPropertyName(elementType.GetProperties().ToArray())));

                            tables.AddRange(GetTableHierarchy(null, tableCols, tableFks, $"{tableName}_{elementType.Name}", typeof(ManyToManyAssignmentTable)));
                             
                        }
                        else
                        {
                            // 1:n relationships
                            // check for element type again, recursive call for non primitves

                            tableCols.Add(new Column($"{model.GetType().Name}{Default.IdProprtyName}", true));;
                            tableFks.Add(new ForeignKey($"{model.GetType().Name}{Default.IdProprtyName}", model.GetType().Name, ModelParser.GetPrimaryKeyPropertyName(model.GetType().GetProperties().ToArray())));

                            if (Types.IsPrimitive(elementType))
                            {

                                tableCols.Add(new Column(Default.IdProprtyName, "PRIMARY KEY"));
                                tableCols.Add(new Column(prop.Name));

                                tables.AddRange(GetTableHierarchy(null, tableCols, tableFks, model.GetType().Name + prop.Name, typeof(PrimitivesChildTable)));
                            }
                            else
                            {
                                tables.AddRange(GetTableHierarchy(Activator.CreateInstance(elementType), tableCols, tableFks));
                            }
                        }

                    }
                    else
                    {
                        // 1:1 relationships
                        tables.AddRange(CreateTables(prop.PropertyType));

                        columns.Add(new Column($"{prop.Name}{Default.IdProprtyName}", true));
                        fks.Add(new ForeignKey($"{prop.Name}{Default.IdProprtyName}", prop.PropertyType.Name, ModelParser.GetPrimaryKeyPropertyName(prop.PropertyType.GetProperties().ToArray())));
                    }

                    continue;
                }

                var col = new Column(prop.Name);
                
                if(Types.HasAttribute<NotNullAttribute>(prop) || !Types.IsNullable(model, prop.Name))
                    col.Constraints.Add("NOT NULL");

                if (Types.HasAttribute<UniqueAttribute>(prop))
                    col.Constraints.Add("UNIQUE");

                if (Types.HasAttribute<DefaultValueAttribute>(prop))
                    col.Constraints.Add("DEFAULT " + prop.GetCustomAttribute<DefaultValueAttribute>().Value);

                columns.Add(col);
            }

        TableBulding:
            tables.Add(new Table() { Name = tableName, Columns = columns, ForeignKeys = fks, Type = type });


            return tables;
        }




    }

    
}
