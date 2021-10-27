using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper
{
    public static class DictionaryExtensions
    {
        public static void Merge<TKey, TValue>(this Dictionary<TKey, TValue> me, Dictionary<TKey, TValue> other)
        {
            other.ToList().ForEach(kvp => me.Add(kvp.Key, kvp.Value));
        }
    }
}
