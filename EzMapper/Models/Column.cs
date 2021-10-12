using System.Collections.Generic;

namespace EzMapper.Models
{
    class Column
    {
        public Column() { }

        public Column(string name, params string[] constraints)
        {
            Name = name;
            Constrains.AddRange(constraints);
        }

        public Column(string name, bool isForeignKey)
        {
            Name = name;
            IsForeignKey = isForeignKey;
        }

        public bool IsForeignKey { get; set; } = false;
        public bool IsPrimaryKey => Constrains.Contains("PRIMARY KEY");
        public string Name { get; set; }
        public string Type { get; set; } = "INTEGER";
        public List<string> Constrains { get; set; } = new();
    }
}
