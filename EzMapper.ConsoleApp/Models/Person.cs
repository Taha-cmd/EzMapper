using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EzMapper.Attributes;

namespace EzMapper.ConsoleApp.Models
{
    public class Person : IModel
    {
        [PrimaryKey]
        public int ID { get; set; }

        [Unique]
        public string FirstName { get; set; }

        [NotNull]
        public string LastName { get; set; }

        [DefaultValue("23")]
        public int Age { get; set; }

        public Car Car { get; set; } = new Car { ModelNumber = 1, Brand = "Volvo" };
    }
}
