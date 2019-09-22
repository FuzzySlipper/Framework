using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    
    [Serializable]
    public abstract class BufferedList : IDisposable {
        
        private static BufferedList<BufferedList> _allLists = new BufferedList<BufferedList>(10,false);

        private bool _addToGlobalList;
        
        public static void UpdateAllLists() {
            _allLists.Update();
            for (int i = 0; i < _allLists.Count; i++) {
                _allLists[i].Update();
            }
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
    public class BufferedList<T> : BufferedList {

        [SerializeField] private int _currentIndex = 0;
        [SerializeField] private ManagedArray<T>[] _list = new ManagedArray<T>[2];
        [SerializeField] private List<T> _pendingDeletes = new List<T>();
        [SerializeField] private List<T> _pendingAdds = new List<T>();
        
        public BufferedList(int size = 10, bool addToLists = true) : base(addToLists) {
            //todo use 2 managed arrays, add function trimcopy, add bufferedlist system to update all bufferedlists 
            _list[0] = new ManagedArray<T>(size);
            _list[1] = new ManagedArray<T>(size);
        }

        private ManagedArray<T> CurrentList { get { return _list[_currentIndex]; } }
        private ManagedArray<T> PreviousList { get { return _list[_currentIndex == 0 ? 1 : 0]; } }
        
        public ref T this[int index] { get { return ref CurrentList[index]; } }
        public int Count { get { return CurrentList.Max; } }

        public void Add(T newVal) {
            _pendingAdds.Add(newVal);
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

        public void ForceAdd() {
            for (int i = 0; i < _pendingAdds.Count; i++) {
                CurrentList.Add(_pendingAdds[i]);
            }
            _pendingAdds.Clear();
        }

        protected override void Update() {
            for (int i = 0; i < _pendingDeletes.Count; i++) {
                CurrentList.Remove(_pendingDeletes[i]);
            }
            _currentIndex = _currentIndex == 0 ? 1 : 0;
            CurrentList.CompressReplaceWith(PreviousList);
            for (int i = 0; i < _pendingAdds.Count; i++) {
                CurrentList.Add(_pendingAdds[i]);
            }
            _pendingAdds.Clear();
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
    }
}
