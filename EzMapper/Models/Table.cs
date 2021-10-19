using System;
using System.Collections.Generic;
using System.Linq;

namespace EzMapper.Models
{
    class Table
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public List<Column> Columns { get; set; } = new();
        public List<ForeignKey> ForeignKeys { get; set; } = new();

        public string PrimaryKey => Columns.Where(col => col.IsPrimaryKey).First().Name;
    }
}
