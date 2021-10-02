using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.ConsoleApp.Models
{
    public class Course
    {
        public int ID { get; set; }
        public List<Student> Students { get; set; } = new();

    }
}
