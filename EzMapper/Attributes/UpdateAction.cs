using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Attributes
{
    public enum UpdateAction
    {
        Cascade,
        NoAction
    }

    public static class UpdateExtensions
    {
        public static string Value(this UpdateAction deleteAction)
        {
            return deleteAction switch
            {
                UpdateAction.Cascade => "CASCADE",
                UpdateAction.NoAction => "NO ACTION",
                _ => throw new NotImplementedException()
            };
        }
    }
}
