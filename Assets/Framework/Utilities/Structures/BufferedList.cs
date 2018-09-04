using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class BufferedList<T> {
        private int _currentIndex = 0;
        private List<T>[] _list = new List<T>[2] {
            new List<T>(), new List<T>()
        };

        public List<T> CurrentList { get { return _list[_currentIndex]; } }
        public List<T> PreviousList { get { return _list[_currentIndex == 0 ? 1 : 0]; } }

        public int Count { get { return CurrentList.Count; } }
        public T this[int index] { get { return CurrentList[index]; } }

        public void Add(T newVal) {
            CurrentList.Add(newVal);
        }

        public void Remove(T newVal) {
            CurrentList.Remove(newVal);
        }

        public void RemoveAt(int index) {
            CurrentList.RemoveAt(index);
        }

        public void Advance() {
            _currentIndex = _currentIndex == 0 ? 1 : 0;
            CurrentList.Clear();
        }

        public void Swap() {
            _currentIndex = _currentIndex == 0 ? 1 : 0;
            CurrentList.Clear(); 
            CurrentList.AddRange(PreviousList);
        }
    }
}
