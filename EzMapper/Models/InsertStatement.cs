using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Models
{
    internal class InsertStatement
    {
        public object Model { get; set; } // actual data
        public Table Table { get; set; } // table structure
        public bool Ignoreable { get; set; } = false; // ignore the record if allready exists
    }
}
