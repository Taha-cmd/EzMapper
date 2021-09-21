using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : Attribute
    {
        private readonly string _parentTable;

        public ForeignKeyAttribute(string parentTable)
        {
            if (string.IsNullOrWhiteSpace(parentTable) || string.IsNullOrEmpty(parentTable))
            {
                throw new ArgumentException($"'{nameof(parentTable)}' cannot be null or whitespace.", nameof(parentTable));
            }

            _parentTable = parentTable;
        }
    }
}
