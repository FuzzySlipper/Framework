using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PointList {
        public HashSet<Point3> HashedCurrent = new HashSet<Point3>();
        public List<Point3> CurrentList = new List<Point3>();

        public Point3 this[int index] { get { return CurrentList[index]; } }
        public int Count { get { return CurrentList.Count; } }

        public void Add(Point3 point) {
            CurrentList.Add(point);
            HashedCurrent.Add(point);
        }

        public void Remove(Point3 newVal) {
            CurrentList.Remove(newVal);
            HashedCurrent.Remove(newVal);
        }

        public void RemoveAt(int index) {
            HashedCurrent.Remove(CurrentList[index]);
            CurrentList.RemoveAt(index);
        }

        public void Clear() {
            CurrentList.Clear();
            HashedCurrent.Clear();
        }

        public bool Contains(Point3 pos) {
            return HashedCurrent.Contains(pos);
        }

        public void TryAdd(Point3 pos) {
            if (!HashedCurrent.Contains(pos)) {
                Add(pos);
            }
        }
    }
}
