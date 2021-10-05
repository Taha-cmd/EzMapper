using EzMapper.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.ConsoleApp.Models
{
    class Product
    {
        public int ID { get; set; }

        [NotNull]
        public string Name { get; set; }

    }
}
