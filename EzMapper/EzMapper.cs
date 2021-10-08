using EzMapper.Attributes;
using EzMapper.Models;
using EzMapper.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EzMapper.Reflection;
using System.Text;

namespace EzMapper
{
    public class EzMapper
    {

        private static readonly List<Type> _types = new();
        private static bool _isBuilt = false;
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

            List<Table> tables = new();
            //List<string> createTableStatements = new();

            _types.ForEach(type => tables.AddRange(CreateTables(type)));

            // in case the user registers a model that is contained in a list within another model,
            // then this model will be created twice (the second time being when the parent model is registered)
            // and the foreign key will not be set correctly
            // this is why we group tables by name (models that were registered more than once)
            // and take the one with the most columns (the correct one with the foreign key)
            tables = tables
                        .GroupBy(table => table.Name)
                        .Select(g => g.OrderByDescending(table => table.Columns.Count))
                        .Select(g => g.First())
                        .OrderByDescending(table => table.ForeignKeys.Count == 0)
                        .ToList(); //https://stackoverflow.com/questions/489258/linqs-distinct-on-a-particular-property


            // the last step will ruin the order and cause tables with foreign keys to come before their parent tables
            // take each table, and find all other tables that reference it (basically find all child tables to a parent table)
            // and append them to the list
            var tmp = new List<Table>();
            foreach (var table in tables)
            {
                tmp.AddRange(
                    tables.Where(
                        t => t.ForeignKeys.Where(fk => fk.TargetTable == table.Name).Count() > 0
                    )
                );
            }

            tables.AddRange(tmp);

            // now we have duplicates, the tables at the end are the correct ones
            tables.Reverse();
            tables = tables.GroupBy(t => t.Name).Select(g => g.First()).ToList();
            tables.Reverse();


            foreach (string statement in StatementBuilder.CreateCreateStatements(tables))
            {
                Console.WriteLine(statement);
            }

            _isBuilt = true;
        }

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

        private static IEnumerable<Table> CreateTables(Type type)
        {
            return CreateTableHierarchy(Activator.CreateInstance(type));
        }
        private static IEnumerable<Table> CreateTableHierarchy(object model, List<Column> cols = null, List<ForeignKey> foreignKeys = null, string tableName = null)
        {
            List<Table> tables = new();
            string primaryKey = string.Empty;
            var props = model?.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).ToList();
            tableName ??= model.GetType().Name;

            var fks = foreignKeys is null ? new List<ForeignKey>() : foreignKeys;
            var columns = cols is null ? new List<Column>() : cols;

            if (model is null)
                goto TableBulding;

            if (Types.HasParentModel(model))
            {
                tables.AddRange(CreateTableHierarchy(Activator.CreateInstance(model.GetType().BaseType)));
            }

            if (!Types.HasParentModel(model))
            {
                primaryKey = SqlTypeInspector.GetPrimaryKeyPropertyName(props.ToArray());
            }
            else
            {
                primaryKey = model.GetType().BaseType.Name + "ID";
                fks.Add(new ForeignKey(primaryKey, model.GetType().BaseType.Name, SqlTypeInspector.GetPrimaryKeyPropertyName(model.GetType().BaseType.GetProperties().ToArray())));
            }

            columns.Add(new Column(primaryKey, "PRIMARY KEY"));

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
                            tables.AddRange(CreateTableHierarchy(Activator.CreateInstance(elementType)));

                            // create assignment table (id, fk1, fk2)
                            tableCols.Add(new Column($"ID", "PRIMARY KEY"));
                            tableCols.Add(new Column($"{tableName}ID"));// reference parent table
                            tableCols.Add(new Column($"{elementType.Name}ID")); // reference child table

                            tableFks.Add(new ForeignKey($"{tableName}ID", model.GetType().Name, primaryKey));
                            tableFks.Add(new ForeignKey($"{elementType.Name}ID", elementType.Name, SqlTypeInspector.GetPrimaryKeyPropertyName(elementType.GetProperties().ToArray())));

                            tables.AddRange(CreateTableHierarchy(null, tableCols, tableFks, $"{tableName}_{elementType.Name}"));
                             
                        }
                        else
                        {
                            // 1:n relationships
                            // check for element type again, recursive call for non primitves

                            tableCols.Add(new Column($"{model.GetType().Name}ID"));;
                            tableFks.Add(new ForeignKey($"{model.GetType().Name}ID", model.GetType().Name, SqlTypeInspector.GetPrimaryKeyPropertyName(model.GetType().GetProperties().ToArray())));

                            if (Types.IsPrimitive(elementType))
                            {

                                tableCols.Add(new Column("ID", "PRIMARY KEY"));
                                tableCols.Add(new Column(prop.Name));

                                tables.AddRange(CreateTableHierarchy(null, tableCols, tableFks, model.GetType().Name + prop.Name));
                            }
                            else
                            {
                                tables.AddRange(CreateTableHierarchy(Activator.CreateInstance(elementType), tableCols, tableFks));
                            }
                        }

                    }
                    else
                    {
                        // 1:1 relationships
                        tables.AddRange(CreateTableHierarchy(prop.GetValue(model)));

                        columns.Add(new Column($"{prop.Name}ID"));
                        fks.Add(new ForeignKey($"{prop.Name}ID", prop.Name, SqlTypeInspector.GetPrimaryKeyPropertyName(prop.GetValue(model).GetType().GetProperties().ToArray())));
                    }

                    continue;
                }

                var col = new Column(prop.Name);
                
                if(Types.HasAttribute<NotNullAttribute>(prop) || !Types.IsNullable(model, prop.Name))
                    col.Constrains.Add("NOT NULL");

                if (Types.HasAttribute<UniqueAttribute>(prop))
                    col.Constrains.Add("UNIQUE");

                if (Types.HasAttribute<DefaultValueAttribute>(prop))
                    col.Constrains.Add("DEFAULT " + prop.GetCustomAttribute<DefaultValueAttribute>().Value);

                columns.Add(col);
            }

        TableBulding:
            tables.Add(new Table() { Name = tableName, Columns = columns, ForeignKeys = fks });

            return tables;
        }




    }

    
}
