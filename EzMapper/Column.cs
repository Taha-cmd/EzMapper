using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper
{
    class Column
    {
        public string Name { get; set; }
        public string Type { get; set; } = "INTEGER";
        public List<string> Constrains { get; set; } = new();
    }
}
