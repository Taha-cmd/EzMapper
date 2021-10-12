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

        public Phone Phone { get; set; } = new Phone() { ID = 3,Brand = "Nokia" };
        public Laptop Laptop { get; set; }
        public List<Book> Books { get; set; } = new();

        
    }
}
