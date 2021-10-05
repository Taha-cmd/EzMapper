using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.ConsoleApp.Models
{
    public class Phone : IModel
    {
        public int ID { get; set; }
        public string Brand { get; set; }
    }
}
