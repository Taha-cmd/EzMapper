using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultValueAttribute : Attribute
    {
        public string Value { get; private set; }

        public DefaultValueAttribute(string value)
        {
            Value = value;
        }
    }
}
