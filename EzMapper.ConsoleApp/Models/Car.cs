using EzMapper.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.ConsoleApp.Models
{
    public class Car : IModel
    {
        [PrimaryKey]
        public int ModelNumber { get; set; }

        public string Brand { get; set; }
    }
}
