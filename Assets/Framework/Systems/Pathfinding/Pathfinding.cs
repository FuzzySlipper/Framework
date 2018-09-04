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

        public static List<Point3> FindUnitPath(Point3 start, Point3 end, Config config, bool ignoreActors) {
            if (start == end) {
                return null;
            }
            var pathfinder = (P3Pathfinder) GetPathfinder<P3Pathfinder>();
            pathfinder.SetConfig(config);
            pathfinder.Start = start;
            pathfinder.End = end;
            if (ignoreActors) {
                pathfinder.WalkableDel = P3Pathfinder.MapCellWalkable;
            }
            else {
                pathfinder.WalkableDel = P3Pathfinder.MapCellWalkableUnitFree;
            }
            var endNode = pathfinder.FindPath();
            if (endNode == null) {
                Store<P3Pathfinder>(pathfinder);
                return null;
            }
            var finalPath = pathfinder.GetPathTrace(endNode);
            Store<P3Pathfinder>(pathfinder);
            return finalPath;
        }

        public static bool CanUnitReach(Point3 start, Point3 end, Config config) {
            var pathfinder = (P3Pathfinder) GetPathfinder<P3Pathfinder>();
            pathfinder.SetConfig(config);
            pathfinder.Start = start;
            pathfinder.End = end;
            pathfinder.WalkableDel = P3Pathfinder.MapCellWalkable;
            bool hasEnd = pathfinder.FindPath() != null;
            Store<P3Pathfinder>(pathfinder);
            return hasEnd;
        }

        public static List<Point3> FindLevelPath(Point3 start, Point3 end, Config config) {
            if (start == end) {
                return null;
            }
            var pathfinder = (P3Pathfinder) GetPathfinder<P3Pathfinder>();
            pathfinder.SetConfig(config);
            pathfinder.Start = start;
            pathfinder.End = end;
            pathfinder.WalkableDel = P3Pathfinder.MapCellExists;
            pathfinder.SetConfig(config);
            var endNode = pathfinder.FindPath();
            if (endNode == null) {
                Store<P3Pathfinder>(pathfinder);
                return null;
            }
            var finalPath = pathfinder.GetPathTrace(endNode);
            Store<P3Pathfinder>(pathfinder);
            return finalPath;
        }

        public static List<Point3> FindSimplePath(Point3 start, Point3 end, Config config) {
            if (start == end) {
                return null;
            }
            var pathfinder = (P3Pathfinder) GetPathfinder<P3Pathfinder>();
            pathfinder.SetConfig(config);
            pathfinder.Start = start;
            pathfinder.End = end;
            pathfinder.WalkableDel = P3Pathfinder.AnyCell;
            pathfinder.SetConfig(config);
            var endNode = pathfinder.FindPath();
            if (endNode == null) {
                Store<P3Pathfinder>(pathfinder);
                return null;
            }
            var finalPath = pathfinder.GetPathTrace(endNode);
            Store<P3Pathfinder>(pathfinder);
            return finalPath;
        }

        public static List<Point3> FindPathCustom(Point3 start, Point3 end, Config config, Func<Point3, Point3, bool> del) {
            if (start == end) {
                return null;
            }
            var pathfinder = (P3Pathfinder) GetPathfinder<P3Pathfinder>();
            pathfinder.SetConfig(config);
            pathfinder.Start = start;
            pathfinder.End = end;
            pathfinder.WalkableDel = del;
            pathfinder.SetConfig(config);
            var endNode = pathfinder.FindPath();
            if (endNode == null) {
                Store<P3Pathfinder>(pathfinder);
                return null;
            }
            var finalPath = pathfinder.GetPathTrace(endNode);
            Store<P3Pathfinder>(pathfinder);
            return finalPath;
        }

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

        public class PriorityQueue<T> where T : IComparable {
            private List<T> m_data;

            public PriorityQueue() {
                m_data = new List<T>();
            }

            public PriorityQueue(PriorityQueue<T> b) {
                m_data = new List<T>(b.m_data);
            }

            public void Enqueue(T queueItem) {
                m_data.Add(queueItem);
                m_data.Sort();
            }

            public void Clear() {
                m_data.Clear();
            }

            public T Dequeue() {
                T frontItem = m_data[0];
                m_data.RemoveAt(0);
                return frontItem;
            }

            public T Peek() {
                T frontItem = m_data[0];
                return frontItem;
            }

            public bool Contains(T queueItem) {
                return m_data.Contains(queueItem);
            }

            public int Count { get { return m_data.Count; } }
        }
    }
}
