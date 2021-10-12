using EzMapper.Models;
using System;
using System.Collections.Generic;
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

    }
}
