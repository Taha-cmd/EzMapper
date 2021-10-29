using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Models
{
    class InsertStatement
    {
        public object Model { get; set; } // actual data
        public Table Table { get; set; } // table structure
        public bool Replaceable { get; set; } = false;
    }
}
