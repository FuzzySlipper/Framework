using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class GenericPools {

        private const int InitialSize = 5;
        private static Dictionary<System.Type, GenericPool> _pools = new Dictionary<Type, GenericPool>();

        public static GenericPool<T> GetPool<T>() where T : class, new() {
            var type = typeof(T);
            if (!_pools.TryGetValue(type, out var pool)) {
                pool = new GenericPool<T>(InitialSize);
                _pools.Add(type, pool);
            }
            return (GenericPool<T>) pool;
        }

        public static void Register<T>(int initialSize, Action<T> clearAction = null, Action<T> oneTime = null) where T : class, new() {
            _pools.AddOrUpdate(typeof(T), new GenericPool<T>(initialSize, clearAction, oneTime));
        }

        public static T New<T>() where T : class, new() {
            return GetPool<T>().New();
        }

        public static void Store<T>(T old) where T : class, new() {
            GetPool<T>().Store(old);
        }
    }
}
