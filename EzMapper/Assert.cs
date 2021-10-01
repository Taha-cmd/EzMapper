using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper
{
    static class Assert
    {
        public static void NotNull<T>(T obj, string name = "object") where T : class
        {
            That(obj is not null, $"{name} can not be null!");
        }
        public static void That(bool expression, string err)
        {
            if (!expression)
                throw new Exception(err);
        }
    }
}
