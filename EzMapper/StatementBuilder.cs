using EzMapper.Database;
using EzMapper.Models;
using EzMapper.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

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
                    // this will add the entire collection  1:n
                    IEnumerable<object> collection = Types.GetCollectionOfType(model, table.Type);
                    if (collection is null) return null;
                    List<InsertStatement> tmpStatements = new();
                    foreach (var obj in collection)
                        tmpStatements.Add(CreateInsertStatement(table, obj));

                    // deal with m:n here
                    if(Types.IsCollectionOfTypeShared(model, table.Type))
                    {
                        //if the collection is shared, we assume that we might insert duplicates, so set the replaceable flag
                        tmpStatements.ForEach(stmt => stmt.Ignoreable = true);

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
                    //if this propety is nested, there might more than one collection
                    // person has cars => each car has a list of primitves
                    // =>find all owerns
                    var targetCollectionPropertyName = table.Columns.Where(col => !col.IsForeignKey && !col.IsPrimaryKey).First().Name;

                    // find owner of the collection
                    string targetTableName = table.ForeignKeys[0].TargetTable;
                    var targetTable = tables.Where(t => t.Name == targetTableName).First();


                    IEnumerable<object> owners = models.Where(m => m?.GetType() == targetTable.Type);
                    table.Columns.Remove(table.Columns.Where(col => col.IsPrimaryKey).First());
                    foreach (object owner in owners)
                    {
                        IList collection = (IList)targetTable.Type.GetProperty(targetCollectionPropertyName).GetValue(owner);
                        if (collection is null) continue;
                        foreach (var primitve in collection)
                        {
                            //TODO: insert all in one statement
                            var obj = new ExpandoObject() as IDictionary<string, object>;
                            obj.Add(targetCollectionPropertyName, primitve);
                            obj.Add(Default.OwnerIdPropertyName, owner.GetType().GetProperty(targetTable.PrimaryKey).GetValue(owner));
                            insertStatements.Add(CreateInsertStatement(table, obj));
                        }
                    }
                }
                else if(!Types.IsPrimitive(table.Type) && !Types.IsCollection(table.Type))// object (1:1)
                {
                    var objects = models.Where(m => m?.GetType().Name == table.Name);

                    foreach (object obj in objects)
                        insertStatements.Add(CreateInsertStatement(table, obj));
                }
            }

            return insertStatements;
            
        }

        private static IEnumerable<Join> GetJoins(Table mainTable, List<Table> tables, Table originalTable, bool ignoreInManyToManyAssignment = false)
        {

            //important: create an alias for the target table on each join to avoid ambiguity
            // do that by cloning the table, since we are dealing with reference types here, creating a new alias will affect all tables

            Assertion.NotNull(mainTable, nameof(mainTable));


            List<Table> tablesClone = new(tables);
            List<Join> joins = new();


            // this will handle cases where the foreign key is in the main table (1:1 and inheritance)
            foreach (var col in mainTable.Columns)
            {
                if (col.IsForeignKey)
                {
                    ForeignKey fk = mainTable.ForeignKeys.Where(f => f.FieldName == col.Name).First();
                    Table target = tablesClone.Where(t => t.Name == fk.TargetTable).First();

                    // only create a join if the maintable contains the target table (actual 1:1 relationship) or the target is a base class
                    // if the foreign key is the other way around, it is a 1:1 relationship
                    if (Types.HasObjectOfType(mainTable.Type, target.Type) || mainTable.Type.IsAssignableTo(target.Type))
                    {
                        // cloning the table will create a new alias, needed if multiple tables join on the same table
                        // example: select student, student has laptop and phone, each laptop and phone have a cpu. the problem will occur when we join on the cpu twice, thus we need a different alias
                        var targetTable = target.Clone();

                        var j = new Join() { Table = mainTable, ForeignKey = col.Name, TargetTable = targetTable, PrimaryKey = targetTable.PrimaryKey };
                        joins.Add(j);
                        joins.AddRange(GetJoins(targetTable, tablesClone, mainTable)); // this takes care of nested objects
                    }

                }
            }



            // this will handle cases where other tables contain a foreign key that points to the main table
            // (1:n and m:n)
            foreach (var table in tablesClone)
            {
                if (table.Name == mainTable.Name) continue; // skip checking relationships between the same table

                foreach (var col in table.Columns)
                {
                    if (col.IsForeignKey)
                    {
                        ForeignKey fk = table.ForeignKeys.Where(f => f.FieldName == col.Name).First();

                        if (fk.TargetTable == mainTable.Name) // if the foreign key points to the current table
                        {
                            if (table.Type == typeof(PrimitivesChildTable))
                            {
                                // this table has 3 cols: value col, pk and fk. fk points to main table's pk
                                // we perform the join from the main table to the collection's table, so we need to reverse the primary and foreign key
                                var targetTable = table.Clone();
                                joins.Add(new Join() { Table = mainTable, ForeignKey = mainTable.PrimaryKey, TargetTable = targetTable, PrimaryKey = fk.FieldName });
                            }
                            else if (table.Type == typeof(ManyToManyAssignmentTable))
                            {
                                if (ignoreInManyToManyAssignment) continue;

                                // join parent to assignment table
                                joins.Add(new Join() { Table = mainTable, ForeignKey = mainTable.PrimaryKey, TargetTable = table, PrimaryKey = fk.FieldName });

                                //find the table of the shared objects
                                string targetTableName = table.Name.Split("_")[1];
                                Table targetTable = tables.Where(t => t.Name == targetTableName).First();

                                // join assignment table to shared objects
                                joins.Add(new Join() { Table = table, PrimaryKey = targetTable.PrimaryKey, TargetTable = targetTable, ForeignKey = targetTableName + Default.IdProprtyName });

                                //make a recursive call in case the shared object has dependencies
                                joins.AddRange(GetJoins(targetTable, tablesClone, targetTable, true));
                            }
                            else if (Types.HasCollectionOfType(mainTable.Type, table.Type))
                            {
                                joins.Add(new Join() { Table = mainTable, ForeignKey = mainTable.PrimaryKey, TargetTable = table, PrimaryKey = fk.FieldName });
                                joins.AddRange(GetJoins(table, tablesClone, mainTable)); // get the dependencies of the object  
                            }
                        }
                    }
                }
            }

            return joins;//.GroupBy(j => j.TargetTable).Select(g => g.First());
        }

        public static SelectStatement CreateSingleSelectStatement<T>() // will return a select statement for the primitves of an object (includes a join to parents if inherited)
        {
            List<Table> tables = SortTablesByForeignKeys(ModelParser.CreateTables(typeof(T)).GroupBy(t => t.Name).Select(g => g.First())).ToList();
            Table mainTable = tables.Where(t => t.Type == typeof(T)).First();
            List<Join> allJoins = GetJoins(mainTable, tables, mainTable).ToList();
            List<Join> joins = new();

            Column pk = mainTable.Columns.Where(col => col.IsPrimaryKey).First();

            var stmt = new SelectStatement(mainTable);

            while (pk.IsForeignKey)
            {
                var fk = mainTable.ForeignKeys.Where(fk => fk.FieldName == pk.Name).First();
                var targetTable = tables.Where(t => t.Name == fk.TargetTable).First();
                joins.Add(allJoins.Where(j => j.Table.Name == mainTable.Name).First());

                mainTable = targetTable;
                pk = targetTable.Columns.Where(col => col.IsPrimaryKey).First();
            }

            stmt.Joins.AddRange(joins);

            return stmt;
        }

        public static IEnumerable<Table> SortTablesByForeignKeys(IEnumerable<Table> tablesArg)
        {
            List<Table> tables = tablesArg.ToList();

            restart:
            for(int i = 0; i < tables.Count; i++)
            {
                for(int j = 0; j < tables.Count; j++)
                {
                    foreach(var col in tables[i].Columns)
                    {
                        if(col.IsForeignKey)
                        {
                            var fk = tables[i].ForeignKeys.Where(f => f.FieldName == col.Name).First();

                            if (fk.TargetTable == tables[j].Name && i < j)
                            {
                                tables.Swap(i, j);
                                goto restart;
                            }
                        }
                    }
                }
            }

            return tables;
        }

        public static SelectStatement CreateSelectStatement(Table mainTable, List<Join> joins)
        {
            return new SelectStatement(mainTable, joins.ToArray());
        }
    }
}
