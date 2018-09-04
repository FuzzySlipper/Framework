using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        public void Remove(T component) {
            var index = _list.FindIndex(component);
            if (index >= 0) {
                Remove(index);
            }
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
                if (cref.Key.IsAssignableFrom(type)) {
                    return true;
                }
            }
            return false;
        }
    }
}
