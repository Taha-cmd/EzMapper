using EzMapper.Attributes;
using EzMapper.Database;
using EzMapper.Models;
using EzMapper.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper
{
    class StatementBuilder
    {
        public static InsertStatement CreateInsertStatement(Table table, object model)
        {
            var statement = new InsertStatement
            {
                Table = table,
                Model = model
                
            };

            return statement;
        }

        public static IEnumerable<InsertStatement> TableToInsertStatements(Table table, object model, Table[] tables)
        {
            //TODO: set replaceable flag for shareable objects
            IEnumerable<object> models = Types.FlattenNestedObjects(model); // retrieve all nested objects and flatten the heirarchy (only for 1:1 relationships)
            var insertStatements = new List<InsertStatement>();

            if (table.Type is not null)
            {
                if (table.Type.IsAssignableFrom(model.GetType()) && table.Type != model.GetType()) // parent model
                {
                    insertStatements.Add(CreateInsertStatement(table, Types.ConvertToParentModel(model)));
                }
                else if (table.Type == model.GetType()) // the model 
                {
                    insertStatements.Add(CreateInsertStatement(table, model));
                }
                else if (Types.HasCollectionOfType(model, table.Type)) // a collection of objects
                {
                    // this will add the entier collection  1:n
                    IEnumerable<object> collection = Types.GetCollectionOfType(model, table.Type);
                    List<InsertStatement> tmpStatements = new();
                    foreach (var obj in collection)
                        tmpStatements.Add(CreateInsertStatement(table, obj));

                    // deal with m:n here
                    if(Types.IsCollectionOfTypeShared(model, table.Type))
                    {
                        //if the collection is shared, we assume that we might insert duplicates, so set the replaceable flag
                        tmpStatements.ForEach(stmt => stmt.Replaceable = true);

                        // find table
                        Table assignmentTable = tables.Where(t => t.Name == $"{model.GetType().Name}_{table.Name}").First();

                        // get rid of primary key
                        assignmentTable.Columns.Remove(assignmentTable.Columns.Where(col => col.IsPrimaryKey).First());

                        //get parent table
                        Table parentTable = tables.Where(t => t.Type == model.GetType()).First();
                        Column pkCol = parentTable.Columns.Where(col => col.IsPrimaryKey).First();
                        string parentPK;

                        //if primary key is also a foriegn key, then we are dealing with inheritance and the pk name is not identical to the pk property name
                        if(pkCol.IsForeignKey)
                        {
                            parentPK = parentTable.ForeignKeys.Where(fk => fk.FieldName == pkCol.Name).First().TargetField;
                        }
                        else
                        {
                            parentPK = pkCol.Name;
                        }


                        //item pk
                        string itemPK = table.Columns.Where(col => col.IsPrimaryKey).First().Name;

                        //find column names of the two fks
                        string fKtoParentTable = assignmentTable.ForeignKeys.Where(fk => fk.TargetTable == parentTable.Name).First().FieldName;
                        string fKtoCurrentTable = assignmentTable.ForeignKeys.Where(fk => fk.TargetTable == table.Name).First().FieldName;

                        foreach (var item in collection)
                        {
                            var obj = new ExpandoObject() as IDictionary<string, object>;
                            obj.Add(fKtoParentTable, model.GetType().GetProperty(parentPK).GetValue(model));
                            obj.Add(fKtoCurrentTable, item.GetType().GetProperty(itemPK).GetValue(item));
                            insertStatements.Add(CreateInsertStatement(assignmentTable, obj));
                        }
                    }

                    insertStatements.AddRange(tmpStatements);

                }
                else if (table.Type == typeof(PrimitivesChildTable)) // a collection of primitives (1:n)
                {
                    var targetCollectionPropertyName = table.Columns.Where(col => !col.IsForeignKey && !col.IsPrimaryKey).First().Name;
                    IList collection = (IList)model.GetType().GetProperty(targetCollectionPropertyName).GetValue(model);

                    table.Columns.Remove(table.Columns.Where(col => col.IsPrimaryKey).First());


                    foreach (var primitve in collection)
                    {
                        var obj = new ExpandoObject() as IDictionary<string, object>;
                        obj.Add(targetCollectionPropertyName, primitve);
                        insertStatements.Add(CreateInsertStatement(table, obj));
                    }
                }
                else // object (1:1)
                {
                    //TODO: handle nested objects
                    var objects = models.Where(m => m?.GetType().Name == table.Name);

                    foreach (object obj in objects)
                        insertStatements.Add(CreateInsertStatement(table, obj));
                }
            }

            return insertStatements;
            
        }

    }
}
