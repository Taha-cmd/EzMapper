using System;

namespace EzMapper
{
    static class Assertion
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
