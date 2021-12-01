using EzMapper.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.ConsoleApp.Models
{
    public class Student : Person, IEzModel
    {
        public string School { get; set; }

        [OnDelete(DeleteAction.Cascade)]
        public List<Phone> Phones { get; set; }

        [OnDelete(DeleteAction.Cascade)]
        public Laptop Laptop { get; set; }

        [OnDelete(DeleteAction.Cascade)]
        public List<Book> Books { get; set; } = new();

        
    }
}
