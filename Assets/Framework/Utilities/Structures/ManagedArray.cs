using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace PixelComrades {

    public abstract class ManagedArray {
        public abstract object Get(int index);
        public abstract void Remove(int index);
    }

    public class ManagedArray<T> : ManagedArray {

        private const int DefaultSize = 32;

        private T[] _list;
        private Stack<int> _freeIndices = new Stack<int>();
        private HashSet<int> _invalidIndices = new HashSet<int>();
        private int _max = 0;
        private int _initialSize;

        public int Max { get { return _max; } }
        public int Count { get { return _max; } }
        public bool IsFull { get { return _max == _list.Length; } }

        public T this[int index] {
            get { return _list[index]; }
            set { _list[index] = value; }
        }

        public override object Get(int index) {
            return _list[index];
        }

        public ManagedArray(int initialSize = DefaultSize) {
            _initialSize = initialSize;
            _list = new T[_initialSize];
        }

        public int Add(T newComponent) {
            if (_max < _list.Length) {
                _list[_max] = newComponent;
                _max++;
                return _max - 1;
            }
            if (_freeIndices.Count > 0) {
                var index = _freeIndices.Pop();
                _invalidIndices.Remove(index);
                _list[index] = newComponent;
                return index;
            }
            _max = _list.Length;
            Array.Resize(ref _list, _max * 2);
            _list[_max] = newComponent;
            _max++;
            return _max - 1;
        }

        public void Replace(ManagedArray<T> source) {
            Clear();
            if (_list.Length < source.Count) {
                Array.Resize(ref _list, source.Count * 2);
            }
            for (int i = 0; i < source.Count; i++) {
                _list[i] = source[i];
            }
            _max = source.Max;
        }

        public void Remove(T component) {
            var index = _list.FindIndex(component);
            if (index >= 0) {
                Remove(index);
            }
        }

        public T Pop() {
            for (int i = 0; i < _max; i++) {
                if (_list[i] != null) {
                    var value = _list[i];
                    Remove(i);
                    return value;
                }
            }
            return default(T);
        }

        public override void Remove(int index) {
            _list[index] = default(T);
            if (_invalidIndices.Contains(index)) {
                return;
            }
            _freeIndices.Push(index);
            _invalidIndices.Add(index);
        }

        public void Clear() {
            for (int i = 0; i < _list.Length; i++) {
                _list[i] = default(T);
            }
            _max = 0;
            _freeIndices.Clear();
            _invalidIndices.Clear();
        }

        public void Reset() {
            _list = new T[_initialSize];
            _invalidIndices.Clear();
            _freeIndices.Clear();
            _max = 0;
        }

        public bool IsInvalid(int index) {
            return _invalidIndices.Contains(index);
        }

        public bool Contains(T value) {
            if (value == null) {
                return false;
            }
            for (int a = 0; a < Max; a++) {
                if (_list[a] == null) {
                    continue;
                }
                if (_list[a].Equals(value)) {
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
            _freeIndices.Clear();
            Array.Sort(_list, comparer);
            for (int i = 0; i < _list.Length; i++) {
                if (_list[i] == null) {
                    _max = i;
                    break;
                }
            }
        }

        /// <summary>
        /// Be careful this allocates
        /// </summary>
        /// <param name="del"></param>
        public void RunAction(Action<T> del) {
            if (_max == 0) {
                return;
            }
            for (int i = 0; i < _max; i++) {
                if (_invalidIndices.Contains(i)) {
                    continue;
                }
                del(_list[i]);
            }
        }

        public void Run(RunDel<T> del) {
            if (_max == 0) {
                return;
            }
            for (int i = 0; i < _max; i++) {
                if (_invalidIndices.Contains(i)) {
                    continue;
                }
                del(_list[i]);
            }
        }


        public delegate void RunDel<TV>(T value) where TV : T;
    }

    public struct ComponentReference {
        public int Index;
        public ManagedArray Array { get; }

        public ComponentReference(int index, ManagedArray array) {
            Index = index;
            Array = array;
        }

        public System.Object Get() {
            return Array.Get(Index);
        }
    }

    public static class ComponentReferenceExtensions {
        //public static bool ContainsType(this IList<ComponentReference> compList, System.Type type) {
        //    for (int i = 0; i < compList.Count; i++) {
        //        if (compList[i].Type == type) {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //public static bool ContainsDerivedType(this IList<ComponentReference> compList, System.Type type) {
        //    for (int i = 0; i < compList.Count; i++) {
        //        if (compList[i].Type.IsAssignableFrom(type)) {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        public static bool ContainsDerivedType(this Dictionary<Type, ComponentReference> compList, System.Type type) {
            foreach (var cref in compList) {
                if (type.IsAssignableFrom(cref.Key)) {
                    return true;
                }
            }
            return false;
        }
    }
}
