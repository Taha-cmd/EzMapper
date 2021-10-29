using EzMapper.ConsoleApp.Models;
using EzMapper.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Tests.Models
{
    class ModelWithDifferentDataTypes
    {
        public int x { get; set; } = 3;
        public int? y { get; set; } = 4;

        [NotNull]
        public string z { get; set; } = "we";

        public string? u { get; set; } = "we";
        public object o { get; set; } = new Student();

        [NotNull]
        public object obj { get; set; } = new Student();

        public List<ModelWithNoAttribute> models {get;set;}
        public List<string> strings { get; set; }
        public List<int> integers { get; set; }

        public object o2 { get; set; } = 3;
        public object l { get; set; } = null;
        public double q { get; set; } = 4.2;
        public double? k { get; set; } = 5.6;
        public double? i { get; set; } = null;
    }
}
