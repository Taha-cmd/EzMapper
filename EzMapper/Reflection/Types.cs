using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace EzMapper.Reflection
{
    public class Types
    {
        public static bool HasParentModel(object model)
        {
            return model.GetType().BaseType.FullName != typeof(object).FullName;
        }

        public static bool IsPrimitive(Type t)
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

            if (IsNullable(obj, propertyName))
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

        public static bool HasAttribute<T>(PropertyInfo prop) where T : Attribute
        {
            return prop.CustomAttributes.Where(attr => attr.AttributeType.Name == typeof(T).Name).ToArray().Length == 1;
        }
    }
}
