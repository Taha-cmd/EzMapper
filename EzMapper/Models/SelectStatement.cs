﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Models
{
    class SelectStatement
    {
        public SelectStatement(Table table, params Join[] joins)
        {
            Table = table;
            Joins.AddRange(joins);
        }
        public Table Table { get; set; } // table structure
        public List<Join> Joins { get; set; } = new();
    }
}
