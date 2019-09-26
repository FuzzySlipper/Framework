using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CircularBuffer<T> {

        //private readonly System.Object _lock = new System.Object();
        private readonly T[] _buffer;
        private readonly float[] _time;
        private int _nextIndex = 0;
        private bool _filledBuffer = false;
        private bool _trackTime;

        public CircularBuffer(int size, bool trackTime) {
            if (size < 1) {
                Debug.LogErrorFormat("Cannot have circular buffer smaller than 1");
                return;
            }
            _buffer = new T[size];
            _trackTime = trackTime;
            if (_trackTime) {
                _time = new float[size];
            }
        }

        public float GetTime(T item) {
            if (!_trackTime) {
                return 0;
            }
            for (int i = 0; i < _buffer.Length; i++) {
                if (_buffer[i].Equals(item)) {
                    return _time[i];
                }
            }
            return 0;
        }

        public void Add(T item) {
            _buffer[_nextIndex] = item;
            if (_trackTime) {
                _time[_nextIndex] = TimeManager.Time;
            }
            _nextIndex++;
            if (_nextIndex >= _buffer.Length) {
                _nextIndex = 0;
                _filledBuffer = true;
            }
        }

        public IEnumerable<T> InOrder() {
            if (_nextIndex == 0 && !_filledBuffer) {
                yield break;
            }
            var startIndex = _nextIndex - 1;
            int index = startIndex;
            while (true) {
                yield return _buffer[index];
                index--;
                if (index < 0) {
                    if (_filledBuffer) {
                        index = _buffer.Length - 1;
                    }
                    else {
                        break;
                    }
                }
                if (index == startIndex) {
                    break;
                }
            }
        }

        public IEnumerable<T> All() {
            for (int i = 0; i < _buffer.Length; i++) {
                if (i > _nextIndex && !_filledBuffer) {
                    break;
                }
                yield return _buffer[i];
            }
        }
    }
    public class CircularBufferPool<T> where T : class, new() {

        //private readonly System.Object _lock = new System.Object();
        private readonly T[] _buffer;
        private int _currentIndex = 0;
        private Action<T> _onetimeInitAction;
        private Action<T> _clearAction;

        public CircularBufferPool(int size, Action<T> clearAction = null, Action<T> oneTime = null) {
            if (size < 1) {
                Debug.LogErrorFormat("Cannot have circular buffer smaller than 1");
                return;
            }
            _buffer = new T[size];
            for (int i = 0; i < size; i++) {
                var t = new T();
                if (_onetimeInitAction != null) {
                    _onetimeInitAction(t);
                }
                _buffer[i] = t;
            }
            _clearAction = clearAction;
            _onetimeInitAction = oneTime;
        }
        
        public T New() {
            //lock (_lock) {
            var newValue = _buffer[_currentIndex];
            _currentIndex++;
            if (_currentIndex >= _buffer.Length) {
                _currentIndex = 0;
            }
            _clearAction?.Invoke(newValue);
            return newValue;
        }
    }
}
