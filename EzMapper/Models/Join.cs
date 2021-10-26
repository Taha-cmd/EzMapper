using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Models
{
    class Join
    {
        public Table Table { get; set; } // join this table
        public string ForeignKey { get; set; } // using this foreign key
        public Table TargetTable { get; set; } // on this table, 
        public string PrimaryKey { get; set; } //using its primary key

        public string Alias1 { get; set; } = RandomString(10);
        public string Alias2 { get; set; } = RandomString(10);

        private static Random random = new Random();
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }


    }
}
