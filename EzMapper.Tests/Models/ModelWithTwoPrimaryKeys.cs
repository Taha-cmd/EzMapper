using EzMapper.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Tests.Models
{
    class ModelWithTwoPrimaryKeys
    {
        [PrimaryKey]
        public int X { get; set; }
        [PrimaryKey]
        public int Y { get; set; }
    }
}
