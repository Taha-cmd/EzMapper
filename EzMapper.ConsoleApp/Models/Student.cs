using EzMapper.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.ConsoleApp.Models
{
    public class Student : Person, IModel
    {
        public string School { get; set; }

        public Car Car { get; set; } = new Car { ID = 1, Brand = "Volvo" };
    }
}
