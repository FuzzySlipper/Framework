using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    
    public class TemplateList {
        
    }

    [System.Serializable]
    public class TemplateList<T> : TemplateList, IEnumerable<T> where T : IEntityTemplate {

        [SerializeField] private ManagedArray<T> _list;

        public TemplateList(int size = 10) {
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
            if (_list.IsInvalid(index) || _list[index].Disposed || _list[index].Entity.Pooled) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Warning: This allocates unless del is cached
        /// </summary>
        /// <param name="del"></param>
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
            return new Enumerator(this);
        }

        public IEnumerator<T> GetEnumerator() {
            return new Enumerator(this);
        }

        public struct Enumerator : IEnumerator<T> {

            private readonly TemplateList<T> _array;
            private int _position;

            public Enumerator(TemplateList<T> array) {
                _array = array;
                _position = -1;
            }

            public void Reset() {
                _position = -1;
            }

            public ref T Current { get { return ref _array[_position]; } }

            System.Object IEnumerator.Current { get { return _array[_position]; } }

            T IEnumerator<T>.Current { get { return _array[_position]; } }

            public void Dispose() {}

            public bool MoveNext() {
                while (true) {
                    unchecked {
                        _position++;
                    }
                    if (_position >= _array.Max) {
                        return false;
                    }
                    if (!_array.IsInvalid(_position)) {
                        return true;
                    }
                }
            }
        }
    }
}
