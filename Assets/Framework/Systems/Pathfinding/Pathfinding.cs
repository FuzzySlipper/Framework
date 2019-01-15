using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static partial class Pathfinding {

        public enum Config {
            Diagonal,
            Vertical,
            Cardinal
        }

        private static Dictionary<System.Type, PathfinderPool> _pools = new Dictionary<System.Type, PathfinderPool>();


        private static AbstractPathfinder GetPathfinder<T>() where T : AbstractPathfinder, new() {
            return GetPool<T>().New();
        }

        private static void Store<T>(AbstractPathfinder pathfinder) where T : AbstractPathfinder, new() {
            GetPool<T>().Store(pathfinder);
        }

        private static PathfinderPool GetPool<T>() where T : AbstractPathfinder, new() {
            var type = typeof(T);
            PathfinderPool pool;
            if (_pools.TryGetValue(type, out pool)) {
                return pool;
            }
            pool = new PathPool<T>();
            _pools.Add(type, pool);
            return pool;
        }

        private abstract class PathfinderPool {

            public abstract void Store(AbstractPathfinder pathfinder);
            public abstract AbstractPathfinder New();
        }

        private class PathPool<T> : PathfinderPool where T : AbstractPathfinder, new() {

            private Queue<T> _objectStack = new Queue<T>();


            public void SetupPool(int initialSize) {
                for (int i = 0; i < initialSize; i++) {
                    _objectStack.Enqueue(new T());
                }
            }

            public override AbstractPathfinder New() {
                if (_objectStack.Count > 0) {
                    var t = _objectStack.Dequeue();
                    return t;
                }
                return new T();
            }

            public override void Store(AbstractPathfinder obj) {
                obj.Clear();
                _objectStack.Enqueue(obj as T);
            }
        }
    }
}
