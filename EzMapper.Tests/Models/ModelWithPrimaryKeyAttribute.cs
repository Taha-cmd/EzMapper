using EzMapper.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Tests.Models
{
    class ModelWithPrimaryKeyAttribute
    {
        [PrimaryKey]
        public int X { get; set; }
        public int ID { get; set; }
        public int ModelId { get; set; }
    }
}
