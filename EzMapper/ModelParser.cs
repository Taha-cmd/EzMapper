using EzMapper.Attributes;
using EzMapper.Database;
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

namespace EzMapper
{
    class ModelParser
    {
        public static string GetPrimaryKeyPropertyName(Type type, params PropertyInfo[] props)
        {
            Assertion.NotNull(props, nameof(props));


            // validate that primary key attribute is used once at most (0 or 1)
            var filteredPropsByAttribute = props.Where(prop => Types.HasAttribute<PrimaryKeyAttribute>(prop)).ToList();
            PropertyInfo pkProp = null;

            if (filteredPropsByAttribute.Count > 1)
                throw new Exception($"Primary Key attribute can be used on only one element!");

            string primaryKeyPropertyName = string.Empty;

            //if one attribute is present, this property is the primary key
            if (filteredPropsByAttribute.Count == 1)
            {
                primaryKeyPropertyName = filteredPropsByAttribute[0].Name;
                pkProp = filteredPropsByAttribute[0];
            }
            else if (filteredPropsByAttribute.Count == 0) // if no attribute is found, search for default name
            {
                var filteredPropsByName = props.Where(prop => prop.Name.ToUpper() == Default.IdProprtyName.ToUpper()).ToList();

                //no key found
                if (filteredPropsByName.Count == 0)
                    throw new Exception($"No candidate for primary key found. No Attribute nor ID Property found");


                primaryKeyPropertyName = filteredPropsByName[0].Name;
                pkProp = filteredPropsByName[0];
            }

            //check for datatype
            if (pkProp.PropertyType != typeof(int))
                throw new Exception($"{primaryKeyPropertyName} is not an integer. Primary key should be an integer");

            return primaryKeyPropertyName;
        }

        public static string GetPkFieldName(Type type)
        {
            string pkPropertyName = GetPrimaryKeyPropertyName(type, type.GetProperties());
            Type ownerType = Types.FindPropertyOwnerType(type, pkPropertyName);

            return ownerType == type ? pkPropertyName : ownerType.Name + Default.IdProprtyName;
        }

        public static IEnumerable<DbParameter> GetDbParams(InsertStatement stmt, IEnumerable<Table> tables, IEnumerable<InsertStatement> insertStatements, IDatebase db)
        {
            //TODO: optimize 1:n of primitves with one insert statement instead of a statement pro value
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

                        if (stmt.Model is ExpandoObject expando)
                        {
                            string pkPropertyName = GetPrimaryKeyPropertyName(possibleOwners.First().Model.GetType(),possibleOwners.First().Model.GetType().GetProperties());
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
                        //if object has a base type, the property might be inherited
                        string pkPropertyName = Types.HasParentModel(obj)
                                ? fk.TargetField.Replace(obj.GetType().BaseType.Name, "")
                                : fk.TargetField;
                        var value = targetTable.Type.GetProperty(pkPropertyName).GetValue(obj);

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

        public static IEnumerable<Table> CreateTables(Type type)
        {
            return GetTableHierarchy(Activator.CreateInstance(type));
        }

        private static IEnumerable<Table> GetTableHierarchy(object model, List<Column> cols = null, List<ForeignKey> foreignKeys = null, string tableName = null, Type type = null, List<string> triggers = null)
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
                primaryKey = GetPrimaryKeyPropertyName(model.GetType(), props.ToArray());
            }
            else
            {
                primaryKey = model.GetType().BaseType.Name + Default.IdProprtyName;
                fks.Add(new ForeignKey(primaryKey, model.GetType().BaseType.Name, GetPrimaryKeyPropertyName(model.GetType().BaseType, model.GetType().BaseType.GetProperties().ToArray()), DeleteAction.Cascade));
                fk = true;
            }

            columns.Add(new Column(primaryKey, "PRIMARY KEY") { IsForeignKey = fk });

            foreach (var prop in props?.Where(prop => prop.Name != primaryKey))
            {
                if (!Types.IsPrimitive(model, prop.Name))
                {
                    //a non primitive could be an object or a collection

                    if (Types.IsCollection(prop.PropertyType))
                    {
                        Type elementType = Types.GetElementType(prop.PropertyType);
                        var tableCols = new List<Column>();
                        var tableFks = new List<ForeignKey>();

                        if (Types.HasAttribute<SharedAttribute>(prop))
                        {

                            //in case of m:n relationships, we must delete the assignment records from the assignment table before deleting the model
                            //to do that, we must add a trigger to the parent

                            //var action = Types.HasAttribute<OnDeleteAttribute>(prop)
                            //    ? prop.GetCustomAttribute<OnDeleteAttribute>().Action
                            //    : throw new Exception($"{prop.Name} of class {prop.DeclaringType} does not have a delete action attribute");

                            string deleteTrigger1 = null; // delete the assignment record
                            string deleteTrigger2 = null; // delete the child object in case no more assignment records point to it


                            deleteTrigger1 = $"CREATE TRIGGER DELETE_ManyToMany_AssignmentRecord_WHEN_{model.GetType().Name}_IS_DELETED ";
                            deleteTrigger1 += $"BEFORE DELETE ON {model.GetType().Name} BEGIN DELETE FROM {model.GetType().Name}_{elementType.Name} WHERE ";
                            deleteTrigger1 += $"{model.GetType().Name}ID ";
                            deleteTrigger1 += $"= old.{GetPkFieldName(model.GetType())}; END;";

                            deleteTrigger2 = $"CREATE TRIGGER DELETE_{elementType.Name}_IF_ALL_REFERENCES_DELETED ";
                            deleteTrigger2 += $"AFTER DELETE ON {model.GetType().Name}_{elementType.Name} WHEN ( SELECT COUNT(*) FROM {model.GetType().Name}_{elementType.Name} WHERE {elementType.Name}ID = old.{elementType.Name}ID ) = 0 ";
                            deleteTrigger2 += $"BEGIN DELETE FROM {elementType.Name} WHERE {GetPkFieldName(elementType)} = old.{elementType.Name}ID; END;";

                            triggers = new() { deleteTrigger1, deleteTrigger2 };


                            // m:n relathionship
                            // when we have m:n relationships, both types are non primitives

                            // create table for the new object
                            tables.AddRange(GetTableHierarchy(Activator.CreateInstance(elementType)));

                            // create assignment table (id, fk1, fk2)
                            tableCols.Add(new Column(Default.IdProprtyName, "PRIMARY KEY"));
                            tableCols.Add(new Column($"{tableName}{Default.IdProprtyName}", true));// reference parent table
                            tableCols.Add(new Column($"{elementType.Name}{Default.IdProprtyName}", true)); // reference child table

                            tableFks.Add(new ForeignKey($"{tableName}{Default.IdProprtyName}", model.GetType().Name, primaryKey, DeleteAction.NoAction));
                            tableFks.Add(new ForeignKey($"{elementType.Name}{Default.IdProprtyName}", elementType.Name, GetPrimaryKeyPropertyName(elementType, elementType.GetProperties().ToArray()), DeleteAction.NoAction) );

                            tables.AddRange(GetTableHierarchy(null, tableCols, tableFks, $"{tableName}_{elementType.Name}", typeof(ManyToManyAssignmentTable)));

                        }
                        else
                        {
                            // 1:n relationships
                            // check for element type again, recursive call for non primitves

                            tableCols.Add(new Column($"{model.GetType().Name}{Default.IdProprtyName}", true));

                            //if the primary key is inherited, it will have a different name
                            string fkTargetField = Types.HasParentModel(model) 
                                ? model.GetType().BaseType.Name + Default.IdProprtyName 
                                : GetPrimaryKeyPropertyName(model.GetType(), model.GetType().GetProperties().ToArray());


                            var action = Types.HasAttribute<OnDeleteAttribute>(prop)
                                ? prop.GetCustomAttribute<OnDeleteAttribute>().Action
                                : throw new Exception($"{prop.Name} of class {prop.DeclaringType} does not have an OnDelete attribute");

                            tableFks.Add(new ForeignKey($"{model.GetType().Name}{Default.IdProprtyName}", model.GetType().Name, fkTargetField, action));

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
                    else // 1:1
                    {
                        var action = Types.HasAttribute<OnDeleteAttribute>(prop)
                            ? prop.GetCustomAttribute<OnDeleteAttribute>().Action
                            : throw new Exception($"{prop.Name} of class {prop.DeclaringType} does not have a delete action attribute");

                        string deleteTrigger = null;

                        if(action == DeleteAction.Cascade)
                        {
                            deleteTrigger = $"CREATE TRIGGER DELETE_{prop.PropertyType.Name}_WHEN_{model.GetType().Name}_IS_DELETED ";
                            deleteTrigger += $"AFTER DELETE ON {model.GetType().Name} BEGIN DELETE FROM {prop.PropertyType.Name} WHERE ";
                            deleteTrigger += $"{prop.PropertyType.Name}.{GetPrimaryKeyPropertyName(prop.PropertyType, prop.PropertyType.GetProperties())} ";
                            deleteTrigger += $"= old.{prop.Name}{Default.IdProprtyName}; END;";
                        }

                        /*List<string>*/ triggers = new() { deleteTrigger };

                        object obj = Activator.CreateInstance(prop.PropertyType);
                        tables.AddRange(GetTableHierarchy(obj, null, null,null,null, null));

                        //make col unique?
                        columns.Add(new Column($"{prop.Name}{Default.IdProprtyName}", true));
                        fks.Add(new ForeignKey($"{prop.Name}{Default.IdProprtyName}", prop.PropertyType.Name, GetPrimaryKeyPropertyName(prop.PropertyType, prop.PropertyType.GetProperties().ToArray()), DeleteAction.NoAction));
                    }

                    continue;
                }

                var col = new Column(prop.Name);

                if (Types.HasAttribute<NotNullAttribute>(prop) || !Types.IsNullable(model, prop.Name))
                    col.Constraints.Add("NOT NULL");

                if (Types.HasAttribute<UniqueAttribute>(prop))
                    col.Constraints.Add("UNIQUE");

                if (Types.HasAttribute<DefaultValueAttribute>(prop))
                    col.Constraints.Add("DEFAULT " + prop.GetCustomAttribute<DefaultValueAttribute>().Value);

                //if (col.IsForeignKey) col.Constraints.Add("ON UPDATE CASCADE");
                columns.Add(col);
            }

        TableBulding:
            tables.Add(new Table() { Name = tableName, Columns = columns, ForeignKeys = fks, Type = type, Triggers = triggers });


            return tables;
        }



    }
}
