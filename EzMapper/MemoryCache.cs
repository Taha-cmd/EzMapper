using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper
{
    internal class MemoryCache : ICache
    {
        private readonly Dictionary<Type, Dictionary<int, object>> _cache = new();
        private readonly ILog _logger = Default.GetLogger();

        public bool Contains<T>(int id)
        {
            if (ContainsType<T>())
                if (_cache[typeof(T)].ContainsKey(id))
                    return true;

            return false;
        }

        public bool ContainsType<T>()
        {
            return _cache.ContainsKey(typeof(T));
        }
        public void Add<T>(int id, T obj)
        {
            if (!_cache.ContainsKey(typeof(T)))
                _cache.Add(typeof(T), new Dictionary<int, object>());

            Assert.That(!Contains<T>(id), $"Object of type {typeof(T)} with id {id} allready exists");

            _logger.Debug($"adding {typeof(T).Name} with id: {id} into cache");
            _cache[typeof(T)].Add(id, obj);
        }

        public void Update<T>(int id, T obj)
        {
            Assert.That(Contains<T>(id), $"Object of type {typeof(T)} with id {id} does not exists");

            _logger.Debug($"updating {typeof(T).Name} with id: {id} in cache");
            _cache[typeof(T)][id] = obj;
        }

        public void Delete<T>(int id)
        {
            Assert.That(Contains<T>(id), $"Object of type {typeof(T)} with id {id} does not exists");

            _logger.Debug($"deleting {typeof(T).Name} with id: {id} from cache");
            _cache[typeof(T)].Remove(id);

        }

        public T Get<T>(int id)
        {
            Assert.That(Contains<T>(id), $"Object of type {typeof(T)} with id {id} does not exists");

            _logger.Debug($"reading {typeof(T).Name} with id: {id} from cache");
            return (T)_cache[typeof(T)][id];
        }

        public IEnumerable<T> Get<T>()
        {
            Assert.That(ContainsType<T>(), $"Objects of type {typeof(T)} not found");

            _logger.Debug($"reading all {typeof(T).Name}s from cache");
            return _cache[typeof(T)].Values.Select(obj => (T)obj).ToList();
        }
    }
}
