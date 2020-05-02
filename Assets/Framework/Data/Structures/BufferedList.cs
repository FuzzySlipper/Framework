using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public class SimpleBufferedList<T> : IEnumerable<T> {

        [SerializeField] private List<T>[] _list;

        public SimpleBufferedList(int size = 10) {
            _list = new[] {
                new List<T>(size), new List<T>(size), new List<T>(size)
            };
        }

        private List<T> CurrentList { get { return _list[0]; } }
        private List<T> PendingAdd { get { return _list[1]; } }
        private List<T> PendingRemove { get { return _list[2]; } }

        
        public T this[int index] { get { return CurrentList[index]; } }
        public int Count { get { return CurrentList.Count; } }

        public void Add(T newVal) {
            PendingAdd.Add(newVal);
        }

        public void Remove(T newVal) {
            PendingRemove.Add(newVal);
        }

        public void Remove(int index) {
            PendingRemove.Add(CurrentList[index]);
        }

        public bool CurrentContains(T obj) {
            return CurrentList.Contains(obj);
        }

        public void Sort(IComparer<T> comparer) {
            CurrentList.Sort(comparer);
        }

        public void Update() {
            for (int i = 0; i < PendingAdd.Count; i++) {
                CurrentList.Add(PendingAdd[i]);
            }
            for (int i = 0; i < PendingRemove.Count; i++) {
                CurrentList.Remove(PendingRemove[i]);
            }
            PendingAdd.Clear();
            PendingRemove.Clear();
        }

        public void Clear() {
            for (int i = 0; i < _list.Length; i++) {
                _list[i].Clear();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return CurrentList.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator() {
            return CurrentList.GetEnumerator();
        }
    }
    
    
    [Serializable]
    public abstract class BufferedList : IDisposable {
        
        private static BufferedList<BufferedList> _allLists = new BufferedList<BufferedList>(50,false);
        private static ManagedArray<BufferedList>.RefDelegate _del = UpdateList;
        private bool _addToGlobalList;
        
        
        public static void UpdateAllLists() {
            _allLists.Update();
            _allLists.Run(_del);
        }

        private static void UpdateList(ref BufferedList list) {
            list.Update();
        }

        protected BufferedList(bool addToGlobalList) {
            _addToGlobalList = addToGlobalList;
            if (_addToGlobalList) {
                _allLists.Add(this);
            }
        }
        
        public void Dispose() {
            if (_addToGlobalList) {
                _allLists.Remove(this);
            }
        }

        protected abstract void Update();
    }
    
    [System.Serializable]
    public class BufferedList<T> : BufferedList, IEnumerable<T> {

        [SerializeField] private int _currentIndex = 0;
        [SerializeField] private ManagedArray<T>[] _list = new ManagedArray<T>[2];
        [SerializeField] private List<T> _pendingDeletes = new List<T>();
        
        public BufferedList(int size = 10, bool addToLists = true) : base(addToLists) {
            _list[0] = new ManagedArray<T>(size);
            _list[1] = new ManagedArray<T>(size);
        }

        private ManagedArray<T> CurrentList { get { return _list[_currentIndex]; } }
        private ManagedArray<T> PreviousList { get { return _list[_currentIndex == 0 ? 1 : 0]; } }
        
        public ref T this[int index] { get { return ref CurrentList[index]; } }
        public int Count { get { return CurrentList.Max; } }
        public int UsedCount { get { return CurrentList.UsedCount; } }

        public void Add(T newVal) {
            CurrentList.Add(newVal);
        }

        public void Remove(T newVal) {
            _pendingDeletes.Add(newVal);
        }

        public void Remove(int index) {
            _pendingDeletes.Add(CurrentList[index]);
        }

        public void Sort(IComparer<T> sorter) {
            CurrentList.Sort(sorter);
        }

        public bool Contains(T obj) {
            return CurrentList.Contains(obj);
        }

        protected override void Update() {
            for (int i = 0; i < _pendingDeletes.Count; i++) {
                CurrentList.Remove(_pendingDeletes[i]);
            }
            _currentIndex = _currentIndex == 0 ? 1 : 0;
            CurrentList.CompressReplaceWith(PreviousList);
            _pendingDeletes.Clear();
        }

//        public void DestructiveAdvance() {
//            _currentIndex = _currentIndex == 0 ? 1 : 0;
//            CurrentList.Clear();
//        }

        public void Clear() {
            CurrentList.Clear();
            PreviousList.Clear();
        }

        public void ClearCurrentAndDeletes() {
            CurrentList.Clear();
            _pendingDeletes.Clear();
        }
        
        /// <summary>
        /// Warning: This allocates unless del is cached
        /// </summary>
        /// <param name="del"></param>
        public void Run(ManagedArray<T>.RefDelegate del) {
            CurrentList.Run(del);
        }

        /// <summary>
        /// Warning: This allocates unless del is cached
        /// </summary>
        /// <param name="del"></param>
        public void Run(ManagedArray<T>.Delegate del) {
            CurrentList.Run(del);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return CurrentList.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator() {
            return CurrentList.GetEnumerator();
        }
    }
}
