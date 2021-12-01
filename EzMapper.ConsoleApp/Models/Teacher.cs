using EzMapper.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.ConsoleApp.Models
{
    public class Teacher : Person
    {
        public bool Retired { get; set; }
        public int WorkingYears { get; set; }

        [Shared] // delete by default, no other attribute is possible
        public List<Course> Courses { get; set; }
    }
}
