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
        public int WorkingYears { get; set; }
        [Shared]
        public List<Course> Courses { get; set; }
    }
}
