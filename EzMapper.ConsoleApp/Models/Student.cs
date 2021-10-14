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

        public List<Phone> Phones { get; set; }
        public Laptop Laptop { get; set; }
        public List<Book> Books { get; set; } = new();

        
    }
}
