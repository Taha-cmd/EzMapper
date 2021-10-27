﻿using EzMapper.Models;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace EzMapper.Database
{
    class SqlStatementBuilder
    {
        public static IEnumerable<string> CreateCreateStatements(IEnumerable<Table> tables)
        {
            var builder = new StringBuilder();

            foreach(var table in tables)
            {
                builder.Append($"CREATE TABLE IF NOT EXISTS {table.Name} (");
                table.Columns.ForEach(col =>
                {
                    builder.Append($" {col.Name} {col.Type} ");
                    col.Constraints.ForEach(constraint => builder.Append($"{constraint} "));
                    builder.Append(',');
                });

                table.ForeignKeys.ForEach(fk =>
                {
                    builder.Append($" FOREIGN KEY({fk.FieldName}) REFERENCES {fk.TargetTable}({fk.TargetField}),");
                });

                builder.Replace(",", "", builder.Length - 1, 1); // get rid of trailing comma
                builder.Append(");");
            }

            return builder.ToString().Split(";");
        }

        public static string CreateInsertStatement(InsertStatement insertStatement)
        {
            var builder = new StringBuilder();

            if (insertStatement.Replaceable)
                builder.Append($"INSERT OR REPLACE INTO {insertStatement.Table.Name} (");
            else
                builder.Append($"INSERT INTO {insertStatement.Table.Name} (");

            insertStatement.Table.Columns.Where(col => !col.Ignored).ToList().ForEach(col => builder.Append(col.Name + ","));
            builder.Replace(",", "", builder.Length - 1, 1); // get rid of trailing comma
            builder.Append(") VALUES (");

            insertStatement.Table.Columns.Where(col => !col.Ignored).ToList().ForEach(col => builder.Append($"@{col.Name},"));
            builder.Replace(",", "", builder.Length - 1, 1); // get rid of trailing comma
            builder.Append(");");

            return builder.ToString();
        }

        public static string CreateSelectStatement(SelectStatement selectStatement)
        {
            var columnsBuilder = new StringBuilder();

            selectStatement.Table.Columns.ForEach(col => columnsBuilder.Append($"{selectStatement.Table.Alias}.{col.Name} AS {selectStatement.Table.Name}_{col.Name},"));

            foreach(var join in selectStatement.Joins)
            {
                join.TargetTable.Columns.ForEach(col => columnsBuilder.Append($"{join.TargetTable.Alias}.{col.Name} AS {join.TargetTable.Name}_{col.Name},"));
            }
            columnsBuilder.Replace(",", "", columnsBuilder.Length - 1, 1); // get rid of trailing comma

            var builder = new StringBuilder($"SELECT {columnsBuilder} FROM {selectStatement.Table.Name} {selectStatement.Table.Alias} ");

            foreach(var join in selectStatement.Joins)
            {
                builder.Append($"LEFT JOIN {join.TargetTable.Name} {join.TargetTable.Alias} ON {join.TargetTable.Alias}.{join.PrimaryKey} = {join.Table.Alias}.{join.ForeignKey} ");
            }

            return builder.ToString();
        }
    }
}
