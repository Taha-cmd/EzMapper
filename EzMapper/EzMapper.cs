using System;
using System.Data.SQLite;
using System.Data.SQLite.Generic;
using System.Data.SqlTypes;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Reflection;
using EzMapper.Attributes;
using System.Collections.Generic;
using System.Collections;

namespace EzMapper
{
    public class EzMapper
    {

        public EzMapper()
        {

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

        public static void TestInheritance(object model)
        {
            Console.WriteLine(model.GetType().BaseType + "; Has parent Model: " + HasParentModel(model));

        }

        private static bool HasParentModel(object model)
        {
            //Console.WriteLine("\n\n" + model.GetType().BaseType.FullName + " =? " +  typeof(object).FullName + "\n\n");
            return model.GetType().BaseType.FullName != typeof(object).FullName;
        }

        //public static Type GetParentModelType(object model)
        //{
           
        //    return model.GetType().BaseType.
        //}

        private static bool IsPrimitive(Type t)
        {
            bool isPrimitiveType = t.IsPrimitive || t.IsValueType || (t == typeof(string));
            return isPrimitiveType;
        }

        public static bool IsPrimitive(object obj, string propertyName)
        {
            if (obj is null)
            {
                return false;
            }

            Type t = obj.GetType().GetProperty(propertyName).PropertyType;

            if(IsNullable(obj, propertyName))
            {
                if (Nullable.GetUnderlyingType(t) is not null)
                    return IsPrimitive(Nullable.GetUnderlyingType(t));
            }
                
            return IsPrimitive(t);
        }

        public static bool IsCollection(Type t)
        {
            if (t == typeof(string))
                return false;

            return typeof(IList).IsAssignableFrom(t) || t.IsArray;
        }

        public static bool IsNullable(object model, string propertyName)
        {
            if (model == null) return true; // obvious
            PropertyInfo prop = model.GetType().GetProperty(propertyName);

            if (prop.PropertyType == typeof(string))
            {
                if (HasAttribute<NotNullAttribute>(prop))
                    return false;
            }

            // https://stackoverflow.com/questions/374651/how-to-check-if-an-object-is-nullable/4131871

            Type type = prop.PropertyType;
            if (!type.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
        }

        public static Type GetElementType(Type type) // returns the element type from a collection type (arrays and lists)
        {
            if (type.IsArray)
                return type.GetElementType();

            return type.GenericTypeArguments[0];
        }

        public static Dictionary<string, string> foriegnKeys = new();

        public static void printForeignKeys()
        {
            foreach(var kvp in foriegnKeys)
            {
                Console.WriteLine("\n\n\n\n\n" + kvp.Key + " references " + kvp.Value + "\n\n\n\n\n");
            }
        }


        public static string[] CreateTable<T>(T model) where T : class
        {
            string statements = CreateTableIntern(model);

            return statements.Split(";");
        }
        private static string CreateTableIntern(object model, List<Column> cols = null, List<ForeignKey> foreignKeys = null, string tableName = null)
        {

            var builder = new StringBuilder();
            string primaryKey = string.Empty;
            var props = model?.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).ToList();
            tableName ??= model.GetType().Name;

            var fks = foreignKeys is null ? new List<ForeignKey>() : foreignKeys;
            var columns = cols is null ? new List<Column>() : cols;

            if (model is null)
                goto TableBulding;

            if (HasParentModel(model))
            {
                //Console.WriteLine("\n\n\n\n sub class detected \n\n\n\n\n" + CreateTableIntern(Activator.CreateInstance(model.GetType().BaseType)));
                builder.Insert(0, CreateTableIntern(Activator.CreateInstance(model.GetType().BaseType)));
            }

            if (!HasParentModel(model))
            {
                primaryKey = GetPrimaryKeyPropertyName(props.ToArray());
            }
            else
            {
                primaryKey = model.GetType().BaseType.Name + "ID";
                fks.Add(new ForeignKey() { FieldName = primaryKey, TargetTable = model.GetType().BaseType.Name, TargetField = GetPrimaryKeyPropertyName(model.GetType().BaseType.GetProperties().ToArray()) });
            }

            columns.Add(new Column() { Name = primaryKey, Constrains = new string[] { "PRIMARY KEY" }.ToList() });

            foreach(var prop in props?.Where(prop => prop.Name != primaryKey))
            {
                if (!IsPrimitive(model, prop.Name))
                {
                    //a non primitive could be an object or a collection

                    if(IsCollection(prop.PropertyType))
                    {
                        // 1:n relationships
                        // check for element type again, recursive call for non primitves
                        Type elementType = GetElementType(prop.PropertyType);
                        var tableCols = new List<Column>();
                        var tableFks = new List<ForeignKey>();

                        tableCols.Add(new Column() { Name = $"{model.GetType().Name}ID" });
                        tableFks.Add(new ForeignKey() { FieldName = $"{model.GetType().Name}ID", TargetTable = model.GetType().Name, TargetField = GetPrimaryKeyPropertyName(model.GetType().GetProperties().ToArray()) });

                        if (IsPrimitive(elementType))
                        {
                            //TODO: create table with only 3 fields (a primary key, a foreign key pointing to owner object and the actual value )

                            tableCols.Add(new Column() { Name = "ID", Constrains = new string[] { "PRIMARY KEY" }.ToList() });
                            tableCols.Add(new Column() { Name = prop.Name });

                            //Console.WriteLine("\n\n\n\n primitve collection detected \n\n\n\n\n" + CreateTableIntern(null, tableCols, tableFks, model.GetType().Name + prop.Name));
                            builder.Append(CreateTableIntern(null, tableCols, tableFks, model.GetType().Name + prop.Name));
                        }
                        else
                        {
                            //TODO: find a way to set foreign key in the collection table (1:n)
                            //Console.WriteLine("\n\n\n\n non primitve collection detected \n\n\n\n\n" + CreateTableIntern(Activator.CreateInstance(elementType), tableCols, tableFks));
                            builder.Append(CreateTableIntern(Activator.CreateInstance(elementType), tableCols, tableFks));
                            //builder.Insert(0, CreateTableIntern(Activator.CreateInstance(elementType), tableCols, tableFks));
                        }
                    }
                    else
                    {
                        // 1:1 relationships
                        //Console.WriteLine("\n\n\n\n non primitve detected \n\n\n\n\n" + CreateTableIntern(prop.GetValue(model)));
                        builder.Insert(0, CreateTableIntern(prop.GetValue(model)));

                        columns.Add(new Column() { Name = $"{prop.Name}ID" });
                        fks.Add(new ForeignKey() { FieldName = $"{prop.Name}ID", TargetTable = prop.Name, TargetField = GetPrimaryKeyPropertyName(prop.GetValue(model).GetType().GetProperties().ToArray()) });
                    }

                    continue;
                }

                var col = new Column() { Name = prop.Name};
                
                if(HasAttribute<NotNullAttribute>(prop) || !IsNullable(model, prop.Name))
                    col.Constrains.Add("NOT NULL");

                if (HasAttribute<UniqueAttribute>(prop))
                    col.Constrains.Add("UNIQUE");

                if (HasAttribute<DefaultMemberAttribute>(prop))
                    col.Constrains.Add("DEFAULT " + prop.GetCustomAttribute<DefaultValueAttribute>().Value);

                columns.Add(col);
            }

            TableBulding:
            builder.Append($"CREATE TABLE IF NOT EXISTS {tableName} (");
            
            foreach (var col in columns)
            {
                builder.Append($" {col.Name} {col.Type} ");
                col.Constrains.ForEach(constraint => builder.Append($"{constraint} "));
                builder.Append(',');
            }

            foreach (var fk in fks)
            {
                //FOREIGN KEY(CardID) REFERENCES Car(ID)
                builder.Append($" FOREIGN KEY({fk.FieldName}) REFERENCES {fk.TargetTable}({fk.TargetField}),");
            }

            builder.Replace(",", "", builder.Length - 1, 1); // get rid of trailing comma
            builder.Append(");");
            return builder.ToString();
        }

        public static bool HasAttribute<T>(PropertyInfo prop) where T : Attribute
        {
            return prop.CustomAttributes.Where(attr => attr.AttributeType.Name == typeof(T).Name).ToArray().Length == 1;
        }

        public static string GetPrimaryKeyPropertyName(params PropertyInfo[] props)
        {
            Assert.NotNull(props, nameof(props));


            // validate that primary key attribute is used once at most (0 or 1)
            var filteredPropsByAttribute = props.Where(prop => HasAttribute<PrimaryKeyAttribute>(prop)).ToList();

            if (filteredPropsByAttribute.Count > 1)
                throw new Exception($"Primary Key attribute can be used on only one element!");

            string primaryKeyPropertyName = string.Empty;

            //if one attribute is present, this property is the primary key
            if(filteredPropsByAttribute.Count == 1)
            {
                primaryKeyPropertyName = filteredPropsByAttribute[0].Name;
            }
            else if(filteredPropsByAttribute.Count == 0) // if no attribute is found, search for default name
            {
                var filteredPropsByName = props.Where(prop => prop.Name.ToUpper() == "ID").ToList();

                //no key found
                if (filteredPropsByName.Count == 0)
                    throw new Exception($"No candidate for primary key found. No Attribute nor ID Property found");

                primaryKeyPropertyName = filteredPropsByName[0].Name;
            }

            //check for datatype
            if (props.Where(prop => prop.Name == primaryKeyPropertyName).First().PropertyType != typeof(int))
                throw new Exception($"{primaryKeyPropertyName} is not an integer. Primary key should be an integer");

            return primaryKeyPropertyName;
        }
    }

    
}
