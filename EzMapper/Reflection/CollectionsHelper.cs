using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Reflection
{
    internal class CollectionsHelper
    {
        public static object ConvertCollection(Type src, Type dest)
        {
            throw new NotImplementedException();
        }

        public static object CreateGenericList(Type elementType)
        {
            Type listType = typeof(List<>).MakeGenericType(elementType);
            return Activator.CreateInstance(listType);
        }

        public static void Add(object instance, object element)
        {

        }

        public static object FillCollection(IEnumerable<object> src, Type targetType)
        {

            IList values = (IList)Activator.CreateInstance(targetType, src.Count());

            for (int i = 0; i < src.Count(); i++)
            {
                if(targetType.IsArray)
                {
                    values[i] = src.ElementAt(i);
                    continue;
                }

                values.Add(src.ElementAt(i));
            }

            return values;
        }

        //public static object ConvertTo(object instance, Type targetType)
        //{
        //    Type srcType = instance.GetType();

        //    if (srcType.IsArray)
        //    {
        //        if (targetType.IsArray)
        //            return instance;

        //        IList list = (List)Activator.CreateInstance(targetType);
                
        //        }
        //}
    }
}
