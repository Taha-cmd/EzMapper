using EzMapper.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.ConsoleApp.Models
{
    public class Cpu
    {
        public int ID { get; set; }
        public string Brand { get; set; }

        [OnDelete(DeleteAction.Cascade)]
        public AluUnit Alu { get; set; }

    }
}
