using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class BufferedList<T> {

        private int _currentIndex = 0;
        private ManagedArray<T>[] _list = new ManagedArray<T>[2];

        public BufferedList(int size = 10) {
            _list[0] = new ManagedArray<T>(size);
            _list[1] = new ManagedArray<T>(size);
        }

        public ManagedArray<T> CurrentList { get { return _list[_currentIndex]; } }
        public ManagedArray<T> PreviousList { get { return _list[_currentIndex == 0 ? 1 : 0]; } }

        public T this[int index] { get { return CurrentList[index]; } }

        public int Count { get { return CurrentList.Count; } }

        public virtual void Add(T newVal) {
            CurrentList.Add(newVal);
        }

        public virtual void Remove(T newVal) {
            CurrentList.Remove(newVal);
        }

        public virtual void RemoveAt(int index) {
            CurrentList.Remove(index);
        }

        public void Advance() {
            _currentIndex = _currentIndex == 0 ? 1 : 0;
            CurrentList.Clear();
        }

        public void Swap() {
            _currentIndex = _currentIndex == 0 ? 1 : 0;
            CurrentList.Replace(PreviousList);
        }

        public virtual void Clear() {
            CurrentList.Clear();
            PreviousList.Clear();
        }
    }
}
