using System;
using System.Data.SQLite;
using System.Data.SQLite.Generic;
using System.Data.SqlTypes;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Reflection;
using EzMapper.Attributes;

namespace EzMapper
{
    public class Class1
    {
        private readonly DbCommandBuilder builder;
        private readonly DbDataAdapter adapter;
        private readonly DbDataReader reader;
        public Class1()
        {
            builder = new SQLiteCommandBuilder();
            adapter = new SQLiteDataAdapter();
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

        private static bool IsPrimitive(Type t)
        {
            bool isPrimitiveType = t.IsPrimitive || t.IsValueType || (t == typeof(string));
            Console.WriteLine($"Is {{ {t} }} Primitive? {isPrimitiveType}");
            return isPrimitiveType;
        }

        public static bool IsPrimitive(object obj, string propertyName)
        {
            if (obj is null)
            {
                Console.WriteLine($"NULL");
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

        public static string CreateTable(object model)
        {
            var props = model.GetType().GetProperties().ToList();
            string primaryKey = GetPrimaryKeyPropertyName(model);

            var builder = new StringBuilder($"CREATE TABLE [IF NOT EXISTS] {model.GetType().Name} (");
            builder.Append($" {primaryKey} INTEGER PRIMARY KEY, ");

            foreach(var prop in props.Where(prop => prop.Name != primaryKey))
            {
                if (!IsPrimitive(model, prop.Name))
                    Console.WriteLine("\n\n\n\n non primitve detected \n\n\n\n\n" + CreateTable(prop.GetValue(model)));

                builder.Append($" {prop.Name} INTEGER");
                builder.Append($" {(HasAttribute<NotNullAttribute>(prop) ? "" : "NOT NULL")}");
                builder.Append($" {(HasAttribute<UniqueAttribute>(prop) ? "UNIQUE" : "")}");
                builder.Append($" {(HasAttribute<DefaultValueAttribute>(prop) ?  "DEFAULT " + prop.GetCustomAttribute<DefaultValueAttribute>().Value : "")}");
                builder.Append(',');
            }


            builder.Replace(",", "", builder.Length - 1, 1); // get rid of trailing comma
            builder.Append(");");
            return builder.ToString();


            
        }

        public static bool HasAttribute<T>(PropertyInfo prop) where T : Attribute
        {
            return prop.CustomAttributes.Where(attr => attr.AttributeType.Name == typeof(T).Name).ToArray().Length == 1;
            //return prop.CustomAttributes.Where(attr => )
        }

        public static string GetPrimaryKeyPropertyName(object model)
        {
            var props = model.GetType().GetProperties().ToList();

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
                    throw new Exception($"{model} does not have a primary key. No Attribute nor ID Property found");

                primaryKeyPropertyName = filteredPropsByName[0].Name;
            }


            //check for datatype
            if (model.GetType().GetProperty(primaryKeyPropertyName).PropertyType != typeof(int))
                throw new Exception($"{primaryKeyPropertyName} is not an integer. Primary key should be an integer");

            return primaryKeyPropertyName;
        }
    }
}
