using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CircularBuffer<T> where T : class, new() {

        //private readonly System.Object _lock = new System.Object();
        private readonly T[] _buffer;
        private int _currentIndex = 0;
        private Action<T> _onetimeInitAction;
        private Action<T> _clearAction;

        public CircularBuffer(int size, Action<T> clearAction = null, Action<T> oneTime = null) {
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
            if (_clearAction != null) {
                _clearAction(newValue);
            }
            return newValue;
        }
    }
}
