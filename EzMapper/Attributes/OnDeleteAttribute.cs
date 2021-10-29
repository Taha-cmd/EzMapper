using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OnDeleteAttribute : Attribute
    {
        public DeleteAction Action { get; }
        public OnDeleteAttribute(DeleteAction action = DeleteAction.Cascade)
        {
            Action = action;
        }
    }
}
