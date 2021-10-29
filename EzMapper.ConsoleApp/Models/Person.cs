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

        [OnDelete(DeleteAction.Cascade)]
        public string[] Hobbies { get; set; }

        [OnDelete(DeleteAction.SetNull)]
        public List<int> Numbers {get; set;} = new();

        [Unique]
        public string FirstName { get; set; }

        [NotNull]
        public string LastName { get; set; }

        [DefaultValue("23")]
        public int Age { get; set; }

        [OnDelete(DeleteAction.Cascade)]
        public Car Car { get; set; }
    }
}
