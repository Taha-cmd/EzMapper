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

    //    public static object ConvertTo(object instance, Type targetType)
    //    {
    //        Type srcType = instance.GetType();

    //        if(srcType.IsArray)
    //        {
    //            if (targetType.IsArray)
    //                return instance;

    //            IList list = (IList)Activator.CreateInstance(targetType);
    //            list.ad
    //        }
    //    }
    }
}
