using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper
{
    interface ICache
    {
        void Add<T>(int id, T obj);
        void Delete<T>(int id);
        void Update<T>(int id, T obj);
        T Get<T>(int id);
        IEnumerable<T> Get<T>();
        bool Contains<T>(int id);
        bool ContainsType<T>();
    }
}
