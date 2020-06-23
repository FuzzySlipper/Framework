using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public class SimpleBufferedList<T> : IEnumerable<T> {

        [SerializeField] private List<T>[] _list = new List<T>[2];
        
        public SimpleBufferedList(int size = 10){
            _list[0] = new List<T>(size);
            _list[1] = new List<T>(size);
        }

        private List<T> CurrentList { get { return _list[0]; } }
        private List<T> Pending { get { return _list[1]; } }
        
        public T this[int index] { get { return CurrentList[index]; } }
        public int Count { get { return CurrentList.Count; } }

        public void Add(T newVal) {
            Pending.Add(newVal);
        }

        public void Remove(T newVal) {
            Pending.Remove(newVal);
        }

        public bool PendingContains(T obj) {
            return Pending.Contains(obj);
        }

        public bool CurrentContains(T obj) {
            return CurrentList.Contains(obj);
        }

        public void Swap() {
            CurrentList.Clear();
            CurrentList.AddRange(Pending);
        }

        public void Clear() {
            CurrentList.Clear();
            Pending.Clear();
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
