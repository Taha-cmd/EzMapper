using EzMapper.Attributes;
using EzMapper.Reflection;
using System;
using System.Linq;
using System.Reflection;

namespace EzMapper.Database
{
    public class SqlTypeInspector
    {
        public static string GetPrimaryKeyPropertyName(params PropertyInfo[] props)
        {
            Assert.NotNull(props, nameof(props));


            // validate that primary key attribute is used once at most (0 or 1)
            var filteredPropsByAttribute = props.Where(prop => Types.HasAttribute<PrimaryKeyAttribute>(prop)).ToList();

            if (filteredPropsByAttribute.Count > 1)
                throw new Exception($"Primary Key attribute can be used on only one element!");

            string primaryKeyPropertyName = string.Empty;

            //if one attribute is present, this property is the primary key
            if (filteredPropsByAttribute.Count == 1)
            {
                primaryKeyPropertyName = filteredPropsByAttribute[0].Name;
            }
            else if (filteredPropsByAttribute.Count == 0) // if no attribute is found, search for default name
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
