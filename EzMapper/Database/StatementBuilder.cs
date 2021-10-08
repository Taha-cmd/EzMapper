using EzMapper.Models;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace EzMapper.Database
{
    class StatementBuilder
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
                    col.Constrains.ForEach(constraint => builder.Append($"{constraint} "));
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
    }
}
