using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Models
{
    internal class UpdateStatement
    {
        public Table Table { get; set; }
        public object Model { get; set; }

    }
}
