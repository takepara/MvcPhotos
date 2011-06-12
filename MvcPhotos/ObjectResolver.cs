using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcPhotos
{
    public static class ObjectResolver
    {
        private static readonly IDictionary<object, HashSet<Func<object>>> _services = new Dictionary<object, HashSet<Func<object>>>();

        public static void Clear()
        {
            _services.Clear();
        }

        public static bool Contains<T>()
        {
            return Contains(typeof (T));
        }

        public static bool Contains(object key)
        {
            return _services.ContainsKey(key);
        }

        public static void Entry<T>(Func<object> resolver)
        {
            Entry(typeof(T), resolver);
        }

        public static void Entry<T>(T key, Func<object> resolver)
        {
            if (!_services.ContainsKey(key))
                _services.Add(key, new HashSet<Func<object>>());

            _services[key].Add(resolver);
        }

        public static T Resolve<T>()
        {
            return Resolve<T>(typeof(T));
        }

        public static T Resolve<T>(object key)
        {
            if (!_services.ContainsKey(key))
                throw new ArgumentOutOfRangeException("key", "指定したkeyは登録されていません");

            var resolver = _services[key].First();
            return (T) resolver();
        }

        public static IEnumerable<T> Resolves<T>()
        {
            return Resolves<T>(typeof(T));
        }

        public static IEnumerable<T> Resolves<T>(object key)
        {
            if (!_services.ContainsKey(key) || _services[key].Count == 0)
                throw new ArgumentOutOfRangeException("key", "指定したkeyは登録されていません");

            return _services[key].Select(resolver => (T)resolver()).ToList();
        }
    }
}
