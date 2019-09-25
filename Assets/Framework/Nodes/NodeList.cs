using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    
    public class NodeList {
        
    }

    [System.Serializable]
    public class NodeList<T> : NodeList, IEnumerable<T> where T : INode {

        [SerializeField] private ManagedArray<T> _list;

        public NodeList(int size = 10) {
            _list = new ManagedArray<T>(size);
        }

        public ref T this[int index] { get { return ref _list[index]; } }
        public int UsedCount { get { return _list.UsedCount; } }
        public int Max { get { return _list.Max; } }

        public void Add(T newVal) {
            _list.Add(newVal);
        }

        public void Remove(T newVal) {
            _list.Remove(newVal);
        }

        public void Remove(int index) {
            _list.Remove(index);
        }

        public bool Contains(T obj) {
            return _list.Contains(obj);
        }

        public void Clear() {
            _list.Clear();
        }

        public bool IsInvalid(int index) {
            if (_list.IsInvalid(index) || _list[index].Disposed) {
                return true;
            }
            return false;
        }

        public void Run(ManagedArray<T>.RefDelegate del) {
            //_list.Run(del);
            if (_list.Max == 0) {
                return;
            }
            for (int i = 0; i < _list.Max; i++) {
                if (IsInvalid(i)) {
                    continue;
                }
                del(ref _list[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _list.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator() {
            return _list.GetEnumerator();
        }
    }
}
