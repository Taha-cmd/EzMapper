using EzMapper.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EzMapper.Reflection
{
    internal class Types
    {
        public static bool HasParentModel(object model)
        {
            return model.GetType().BaseType.FullName != typeof(object).FullName;
        }

        public static bool HasParentModel(Type t)
        {
            return HasParentModel(Activator.CreateInstance(t));
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
                if (HasAttribute<Attributes.NotNullAttribute>(prop))
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

        public static Type FindPropertyOwnerType(Type type, string propertyName)
        {
            return type.GetProperty(propertyName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance) is null 
                ? FindPropertyOwnerType(type.BaseType, propertyName) 
                : type;
        }

        public static object ConvertToParentModel(object model)
        {
            if (!HasParentModel(model))
                throw new Exception($"Object of type {model} is not a derived type");

            object parent = Activator.CreateInstance(model.GetType().BaseType);

            foreach(var prop in parent.GetType().GetProperties())
            {
                prop.SetValue(parent, prop.GetValue(model));
            }

            return parent;
        }

        public static bool HasCollectionOfType(Type parent, Type member)
        {
            return HasCollectionOfType(Activator.CreateInstance(parent), member);
        }

        public static bool HasCollectionOfType(object model, Type type)
        {
            var props = model.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            var collectionTypeProps = props.Where(prop => IsCollection(prop.PropertyType)).Where(prop => GetElementType(prop.PropertyType) == type);

            return collectionTypeProps.ToList().Count >= 1;
        }

        public static bool HasCollectionOfPrimitives(object model)
        {
            var props = model.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            var collectionTypeProps = props.Where(prop => IsCollection(prop.PropertyType)).Where(prop => IsPrimitive(GetElementType(prop.PropertyType)));

            return collectionTypeProps.ToList().Count >= 1;
        }

        public static IEnumerable<object> GetCollectionOfType(object model, Type type)
        {
            if (!HasCollectionOfType(model, type))
                throw new Exception($"object of type {model} does not have a collection of type {type}");

            var props = model.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            var collectionTypeProp = props.Where(prop => IsCollection(prop.PropertyType)).Where(prop => GetElementType(prop.PropertyType) == type).First();

            return (IEnumerable<object>)collectionTypeProp.GetValue(model);
        }

        public static bool IsCollectionOfTypeShared(object model, Type type)
        {
            if (!HasCollectionOfType(model, type))
                throw new Exception($"object of type {model} does not have a collection of type {type}");

            var props = model.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            var collectionTypeProp = props.Where(prop => IsCollection(prop.PropertyType)).Where(prop => GetElementType(prop.PropertyType) == type).First();

            return HasAttribute<SharedAttribute>(collectionTypeProp);
        }

        public static IEnumerable<object> FlattenNestedObjects(object model)
        {
            List<object> models = new();

            if (model is null)
                return models;

            var props = model.GetType().GetProperties();

            foreach(var prop in props)
            {
                if(!IsPrimitive(prop.PropertyType) && !IsCollection(prop.PropertyType))
                {
                    models.Add(prop.GetValue(model));
                    models.AddRange(FlattenNestedObjects(prop.GetValue(model)));
                }

                if(IsCollection(prop.PropertyType))
                {
                    if(!IsPrimitive(GetElementType(prop.PropertyType)))
                    {
                        IList collection = (IList)prop.GetValue(model);
                        if (collection is null) continue;
                        foreach (var nestedObject in collection)
                        {
                            models.Add(nestedObject);
                            models.AddRange(FlattenNestedObjects(nestedObject));
                        }
                            
                    }
                }
            }

            if (HasParentModel(model))
            {
                models.Add(ConvertToParentModel(model));
            }
                

            return models;
        }

        public static bool HasObjectOfType(object parent, Type child)
        {
            var props = parent.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            return props.Any(prop => prop.PropertyType == child);
        }

        public static bool HasObjectOfType(Type parent, Type child)
        {
            return HasObjectOfType(Activator.CreateInstance(parent), child);
        }

        public static IEnumerable<PropertyInfo> GetPrimitiveProperties<T>() // includes inherited properties
        {
            return typeof(T).GetProperties()
                .Where(prop => IsPrimitive(prop.PropertyType));
        }

        public static object InvokeGenericMethod(Type classType, object instance, string methodName, Type typeArugment, params object[] arguments)
        {
            
            Type[] types = arguments.Select(arg => arg.GetType()).ToArray();
            CallingConventions callingConvection = instance is null ? CallingConventions.Standard : CallingConventions.HasThis;
            BindingFlags flag = instance is null ? BindingFlags.Static : BindingFlags.Instance;

            var methodInfo = classType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | flag, null, callingConvection, types, null); // get private methods also

            if (methodInfo is null)
                throw new Exception("Something went wrong");

            var genericMethodInfo = methodInfo.MakeGenericMethod(typeArugment);
            return genericMethodInfo.Invoke(instance, arguments);
        }

        public static IEnumerable<Type> GetSubTypes<T>(params Type[] types)
        {
            var subTypes = new List<Type>();

            foreach (Type type in types)
            {
                if (type.IsAssignableTo(typeof(T)) && type != typeof(T))
                    subTypes.Add(type);
            }

            return subTypes;
        }
    }
}
