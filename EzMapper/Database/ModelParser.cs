using EzMapper.Attributes;
using EzMapper.Models;
using EzMapper.Reflection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EzMapper.Tests")]

namespace EzMapper.Database
{
    class ModelParser
    {
        public static string GetPrimaryKeyPropertyName(params PropertyInfo[] props)
        {
            Assertion.NotNull(props, nameof(props));


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
                var filteredPropsByName = props.Where(prop => prop.Name.ToUpper() == Default.IdProprtyName.ToUpper()).ToList();

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

        public static IEnumerable<DbParameter> GetDbParams(InsertStatement stmt, IEnumerable<Table> tables, IEnumerable<InsertStatement> insertStatements, IDatebase db)
        {
            List<DbParameter> paras = new();

            foreach (Column col in stmt.Table.Columns)
            {

                if (col.IsForeignKey && stmt.Table.Type != typeof(ManyToManyAssignmentTable))
                {

                    var fk = stmt.Table.ForeignKeys.Where(fk => fk.FieldName == col.Name).First(); // find the foriegn key
                    var targetTable = tables.Where(t => t.Name == fk.TargetTable).First(); // find the target table

                    object obj;
                    if (Types.HasObjectOfType(stmt.Model, targetTable.Type)) // if the object contains the nested object, find the nested object from the current object
                    {
                        // we can deduce the field name based on the foreign key, since foreign keys are created based on conventions and the use cannot influence it
                        // for example: if the object Laptop has a reference to a property called CPU, then the foreign key will be called CPUID
                        // thus, a foreign key of the name CarId for example refers to a property called Car
                        string propertyName = col.Name.Replace(Default.IdProprtyName, "");
                        obj = stmt.Model.GetType().GetProperty(propertyName).GetValue(stmt.Model);
                    }
                    else
                    {
                        var possibleOwners = insertStatements.Where(stmt => stmt.Table.Name == targetTable.Name); // find the correct object

                        if (stmt.Model is ExpandoObject)
                        {
                            var expando = (ExpandoObject)stmt.Model;

                            string pkPropertyName = GetPrimaryKeyPropertyName(possibleOwners.First().Model.GetType().GetProperties());
                            int ownerPKValue = (int)expando.Where(el => el.Key == Default.OwnerIdPropertyName).First().Value;
                            obj = possibleOwners.Where(ow => (int)ow.Model.GetType().GetProperty(pkPropertyName).GetValue(ow.Model) == ownerPKValue).First().Model;
                        }
                        else
                        {
                            obj = possibleOwners.First().Model;
                        }


                        
                    }

                    if (obj is not null)
                    {
                        var value = targetTable.Type.GetProperty(fk.TargetField).GetValue(obj);

                        paras.Add(db.Param(col.Name, value));
                    }
                    else
                    {
                        col.Ignored = true;
                    }

                }
                else
                {

                    if (stmt.Model is ExpandoObject @object)
                    {
                        paras.Add(db.Param(col.Name, @object.Where(el => el.Key == col.Name).First().Value));
                    }
                    else
                    {
                        paras.Add(db.Param(col.Name, stmt.Table.Type.GetProperty(col.Name).GetValue(stmt.Model)));

                    }
                }

            }

            return paras;
        }
    }
}
