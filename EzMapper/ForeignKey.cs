using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper
{
    class ForeignKey
    {
        public string FieldName { get; set; }
        public string TargetTable { get; set; }
        public string TargetField { get; set; }
    }
}
