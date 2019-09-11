using System;
using System.Collections.Generic;

namespace SensorToolkit {
    public class ObjectCache<T> {
        private Stack<T> cache;

        public ObjectCache() : this(10) {
        }

        public ObjectCache(int startSize) {
            cache = new Stack<T>();
            for (int i = 0; i < startSize; i++) {
                cache.Push(Create());
            }
        }

        protected virtual T Create() {
            return Activator.CreateInstance<T>();
        }

        public virtual void Dispose(T obj) {
            cache.Push(obj);
        }

        public T Get() {
            if (cache.Count > 0) {
                return cache.Pop();
            }
            return Create();
        }
    }

    public class ListCache<T> : ObjectCache<List<T>> {
        public override void Dispose(List<T> obj) {
            obj.Clear();
            base.Dispose(obj);
        }
    }
}