using System;
using System.Collections.Generic;
using System.Linq;

namespace EzMapper.Models
{
    class Table
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public List<Column> Columns { get; set; } = new();
        public List<ForeignKey> ForeignKeys { get; set; } = new();
        public List<string> Triggers { get; set; }

        public string Alias { get; set; } = RandomString(10);

        public string PrimaryKey => Columns.Where(col => col.IsPrimaryKey).First().Name;

        private static Random random = new();
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public Table Clone()
        {
            return new Table() { Type = this.Type, Name = Name, Columns = new(Columns), ForeignKeys = new(ForeignKeys), Alias = RandomString(10) };
        }
    }
}
