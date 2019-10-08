using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Object = System.Object;

namespace PixelComrades {

    public abstract class ManagedArray {
        public abstract System.Type ArrayType { get; }
        public abstract object Get(int index);
        public abstract void Remove(int index);
        public abstract bool IsInvalid(int index);
    }

    public class ManagedArray<T> : ManagedArray, IEnumerable<T>, ISerializable {

        private const int DefaultSize = 32;

        private bool _isValueType;
        private T[] _array;
        private int _max = 0;
        private List<int> _freeIndices;
        private bool[] _invalidPositionIndex;

        public int Max { get { return _max; } }
        public int ArrayCount { get { return _array.Length; } }
        public int UsedCount { get { return _max - _freeIndices.Count; } }
        public bool IsFull { get { return _max == _array.Length && _freeIndices.Count == 0; } }
        public override System.Type ArrayType { get { return typeof(T); } }
        public ref T this[int index] { get { return ref _array[index]; } }

        public ManagedArray(int initialSize = DefaultSize) {
            _array = new T[initialSize];
            _invalidPositionIndex = new bool[initialSize];
            _freeIndices = new List<int>(initialSize);
            _isValueType = typeof(T).IsValueType;
        }

        public ManagedArray(SerializationInfo info, StreamingContext context) {
            _array = info.GetValue(nameof(_array), _array);
            _max = info.GetValue(nameof(_max), _max);
            _invalidPositionIndex = info.GetValue(nameof(_invalidPositionIndex), _invalidPositionIndex);
            _freeIndices = info.GetValue(nameof(_freeIndices), _freeIndices);
            _isValueType = typeof(T).IsValueType;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_array), _array);
            info.AddValue(nameof(_max), _max);
            info.AddValue(nameof(_invalidPositionIndex), _invalidPositionIndex);
            info.AddValue(nameof(_freeIndices), _freeIndices);
        }

        public override object Get(int index) {
            return _array[index];
        }

        public void Set(int index, T item) {
            _array[index] = item;
            _freeIndices.Remove(index);
            _invalidPositionIndex[index] = false;
        }

        public bool IndexFree(int index) {
            return index < _invalidPositionIndex.Length && !_invalidPositionIndex[index];
        }

        public bool HasIndex(int index) {
            return _array.HasIndex(index);
        }

        public int Add(T newComponent) {
            if (_freeIndices.Count > 0) {
                var index = _freeIndices[_freeIndices.Count - 1];
                _freeIndices.RemoveAt(_freeIndices.Count - 1);
                _array[index] = newComponent;
                _invalidPositionIndex[index] = false;
                return index;
            }
            if (_max < _array.Length) {
                _array[_max] = newComponent;
                _max++;
                return _max - 1;
            }
            ResizeArray(_max *2);
            _array[_max] = newComponent;
            _max++;
            return _max - 1;
        }

        private void ResizeArray(int newCount) {
            System.Array.Resize(ref _array, newCount);
            System.Array.Resize(ref _invalidPositionIndex, newCount);
        }

        public void Replace(ManagedArray<T> source) {
            Clear();
            if (_array.Length < source.Max) {
                ResizeArray(source.Max * 2);
            }
            for (int i = 0; i < source.Max; i++) {
                _array[i] = source[i];
            }
            _max = source._max;
        }

        public void CompressReplaceWith(ManagedArray<T> source) {
            Clear();
            if (_array.Length <= source.UsedCount + 2) {
                ResizeArray(source.UsedCount * 2);
            }
            for (int i = 0; i < source.Max; i++) {
                if (source._invalidPositionIndex[i]) {
                    continue;
                }
                if (_max >= _array.Length || i >= source._array.Length) {
                    Debug.LogFormat("Something went wrong max {0} {1} i {2} {3}", _max, _array.Length, i, source._array.Length);
                    ResizeArray(_array.Length * 2);
                }
                _array[_max] = source._array[i];
                _max++;
            }
        }

        public void Remove(T component) {
            var index = _array.FindIndex(component);
            if (index >= 0) {
                Remove(index);
            }
        }

        public T Pop() {
            for (int i = 0; i < _max; i++) {
                if (_array[i] != null) {
                    var value = _array[i];
                    Remove(i);
                    return value;
                }
            }
            return default(T);
        }

        public override void Remove(int index) {
            _array[index] = default(T);
            if (!_invalidPositionIndex[index]) {
                _freeIndices.Add(index);
                _invalidPositionIndex[index] = true;
            }
        }

        public void Clear() {
            for (int i = 0; i < _array.Length; i++) {
                _array[i] = default(T);
            }
            for (int i = 0; i < _invalidPositionIndex.Length; i++) {
                _invalidPositionIndex[i] = false;
            }
            _max = 0;
            _freeIndices.Clear();
        }

        public void Reset() {
            _array = new T[_array.Length];
            _invalidPositionIndex = new bool[_array.Length];
            _freeIndices.Clear();
            _max = 0;
        }

        public override bool IsInvalid(int index) {
            return _invalidPositionIndex[index];
        }

        public bool Contains(T value) {
            if (value == null) {
                return false;
            }
            for (int a = 0; a < _max; a++) {
                if (_array[a] == null) {
                    continue;
                }
                if (_array[a].Equals(value)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Must sort nulls to top
        /// </summary>
        /// <param name="comparer"></param>
        public void Sort(IComparer<T> comparer) {
            if (_isValueType) {
                Console.Log(string.Format("Attempted to sort value type {0} which does not work"));
            }
            _freeIndices.Clear();
            System.Array.Sort(_array, comparer);
            for (int i = 0; i < _array.Length; i++) {
                if (_array[i] == null) {
                    _max = i;
                    break;
                }
            }
        }

        /// <summary>
        /// Warning: This allocates unless del is cached
        /// </summary>
        /// <param name="del"></param>
        public void Run(RefDelegate del) {
            if (_max == 0) {
                return;
            }
            for (int i = 0; i < _max; i++) {
                if (_invalidPositionIndex[i]) {
                    continue;
                }
                if (_array[i] == null) {
                    Debug.LogErrorFormat("Something went wrong at {0} for {1}", i, ArrayType);
                    continue;
                }
                del(ref _array[i]);
            }
        }

        /// <summary>
        /// Warning: This allocates unless del is cached
        /// </summary>
        /// <param name="del"></param>
        public void Run(Delegate del) {
            if (_max == 0) {
                return;
            }
            for (int i = 0; i < _max; i++) {
                if (_invalidPositionIndex[i]) {
                    continue;
                }
                del(_array[i]);
            }
        }

        public delegate void RefDelegate(ref T value);

        public delegate void Delegate(T value);

        public Enumerator GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new Enumerator(this);
        }

        public struct Enumerator : IEnumerator<T> {

            private readonly ManagedArray<T> _array;
            private int _position;

            public Enumerator(ManagedArray<T> array) {
                _array = array;
                _position = -1;
            }

            public void Reset() {
                _position = -1;
            }

            public ref T Current { get { return ref _array[_position]; } }

            Object IEnumerator.Current { get { return _array[_position]; } }

            T IEnumerator<T>.Current { get { return _array[_position]; } }

            public void Dispose() {
            }

            private static void ThrowInvalidOp() => throw new InvalidOperationException();

            public bool MoveNext() {
                while (true) {
                    unchecked {
                        _position++;
                    }
                    if (_position >= _array._max) {
                        return false;
                    }
                    if (!_array._invalidPositionIndex[_position]) {
                        return true;
                    }
                }
            }
        }
    }

}
