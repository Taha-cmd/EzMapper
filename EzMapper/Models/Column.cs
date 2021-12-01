using System.Collections.Generic;

namespace EzMapper.Models
{
    internal class Column
    {
        public Column() { }

        public Column(string name, params string[] constraints)
        {
            Name = name;
            Constraints.AddRange(constraints);
        }

        public Column(string name, bool isForeignKey)
        {
            Name = name;
            IsForeignKey = isForeignKey;
        }

        public bool Ignored { get; set; } = false;
        public bool IsForeignKey { get; set; } = false;
        public bool IsPrimaryKey => Constraints.Contains("PRIMARY KEY");
        public string Name { get; set; }
        public string Type => IsPrimaryKey ? "INTEGER" : "TEXT";
        public List<string> Constraints { get; set; } = new();
    }
}
