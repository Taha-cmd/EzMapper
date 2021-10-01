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

namespace EzMapper
{
    public class Class1
    {

        public Class1()
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

        public static Dictionary<string, string> foriegnKeys = new();

        public static void printForeignKeys()
        {
            foreach(var kvp in foriegnKeys)
            {
                Console.WriteLine("\n\n\n\n\n" + kvp.Key + " references " + kvp.Value + "\n\n\n\n\n");
            }
        }

        public static string CreateTable<T>(T model) where T : class
        {
            //TODO: create foreign keys
            var builder = new StringBuilder();
            string primaryKey = string.Empty;

            if (HasParentModel(model))
                Console.WriteLine("\n\n\n\n sub class detected \n\n\n\n\n" + CreateTable(Activator.CreateInstance(model.GetType().BaseType)));

            var props = model.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).ToList();

            if (!HasParentModel(model))
                primaryKey = GetPrimaryKeyPropertyName(props.ToArray());
            else
                primaryKey = "ID";

            builder.Append($"CREATE TABLE [IF NOT EXISTS] {model.GetType().Name} (");
            builder.Append($" {primaryKey} INTEGER PRIMARY KEY, ");

            var fks = new List<ForeignKey>();

            foreach(var prop in props.Where(prop => prop.Name != primaryKey))
            {
                if (!IsPrimitive(model, prop.Name))
                {
                    Console.WriteLine("\n\n\n\n non primitve detected \n\n\n\n\n" + CreateTable(prop.GetValue(model)));
                    builder.Append($" {prop.Name}ID INTEGER, ");
                    fks.Add(new ForeignKey() { FieldName = $"{prop.Name}ID", TargetTable = prop.Name, TargetField = GetPrimaryKeyPropertyName(prop.GetValue(model).GetType().GetProperties().ToArray()) });
                    continue;
                }

                builder.Append($" {prop.Name} INTEGER");
                builder.Append($" {(HasAttribute<NotNullAttribute>(prop) || !IsNullable(model, prop.Name) ? "NOT NULL" : " ")}");
                builder.Append($" {(HasAttribute<UniqueAttribute>(prop) ? "UNIQUE" : "")}");
                builder.Append($" {(HasAttribute<DefaultValueAttribute>(prop) ?  "DEFAULT " + prop.GetCustomAttribute<DefaultValueAttribute>().Value : "")}");
                builder.Append(',');
            }

            //FOREIGN KEY(CardID) REFERENCES Car(ID)

            foreach (var fk in fks)
            {
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
