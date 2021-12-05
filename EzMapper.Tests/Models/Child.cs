using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Tests.Models
{
    class Child<T> : Parent
    {
        public List<T> Collection { get; set; } = new();
    }
}
