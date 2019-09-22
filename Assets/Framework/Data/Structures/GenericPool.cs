using System;
using System.Collections.Generic;


public class GenericPool<T> where T : class, new() {
    private Queue<T> _objectStack;
    private Action<T> _onetimeInitAction;
    private Action<T> _clearAction;

    public GenericPool(int initialSize, Action<T> clearAction = null, Action<T> oneTime = null) {
        _objectStack = new Queue<T>(initialSize);
        _clearAction = clearAction;
        _onetimeInitAction = oneTime;
    }

    public T New() {
        if (_objectStack.Count > 0) {
            var t = _objectStack.Dequeue();
            return t;
        }
        else {
            var t = new T();
            if (_onetimeInitAction != null) {
                _onetimeInitAction(t);
            }
            return t;
        }
    }

    public void Store(T obj) {
        if (_clearAction != null) {
            _clearAction(obj);
        }
        _objectStack.Enqueue(obj);
    }
}