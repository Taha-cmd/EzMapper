using EzMapper.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper
{
    class Table
    {
        public string Name { get; set; }
        public List<Column> Columns { get; set; }
        public List<ForeignKey> ForeignKeys { get; set; }
    }
}
