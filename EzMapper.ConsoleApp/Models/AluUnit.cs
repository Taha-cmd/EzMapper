using EzMapper.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.ConsoleApp.Models
{
    public class AluUnit
    {
        public int ID { get; set; }
        public string PlaceHolder { get; set; }

        [OnDelete(DeleteAction.Cascade)]
        public string[] ListOfStuff { get; set; }
    }
}
