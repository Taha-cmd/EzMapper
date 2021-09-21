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

        [ForeignKey("Car")]
        public int CarID { get; set; }
    }
}
