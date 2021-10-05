using EzMapper.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.ConsoleApp.Models
{
    class Order
    {
        public int ID { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        [Shared]
        public List<Product> Products { get; set; } = new();
    }
}
