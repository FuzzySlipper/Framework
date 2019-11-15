using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace PixelComrades {
    
    public class SortByPriorityClass : IComparer<System.Type> {
        
        Dictionary<System.Type, int> _priority = new Dictionary<Type, int>();

        public int Compare(System.Type first, System.Type second) {
            if (first == null && second == null) {
                return 0;
            }
            if (first == null) {
                return 1;
            }
            if (second == null) {
                return -1;
            }
            if (!_priority.TryGetValue(first, out var firstPriority)) {
                var firstAttr = first.GetCustomAttribute<PriorityAttribute>(true);
                if (firstAttr != null) {
                    _priority.Add(first, firstAttr.Value);
                }
                else {
                    _priority.Add(first, (int) Priority.Normal);
                }
            }
            if (!_priority.TryGetValue(second, out var secondPriority)) {
                var secondAttr = second.GetCustomAttribute<PriorityAttribute>(true);
                if (secondAttr != null) {
                    _priority.Add(second, secondAttr.Value);
                }
                else {
                    _priority.Add(second, (int) Priority.Normal);
                }
            }
            return firstPriority.CompareTo(secondPriority);
            
        }
    }

    public class SortByPriorityObject : IComparer<System.Object> {

        Dictionary<System.Type, int> _priority = new Dictionary<Type, int>();

        public int Compare(System.Object first, System.Object second) {
            if (first == null && second == null) {
                return 0;
            }
            if (first == null) {
                return 1;
            }
            if (second == null) {
                return -1;
            }
            var firstType = first.GetType();
            var secondType = second.GetType();
            if (!_priority.TryGetValue(firstType, out var firstPriority)) {
                var firstAttr = firstType.GetCustomAttribute<PriorityAttribute>(true);
                if (firstAttr != null) {
                    _priority.Add(firstType, firstAttr.Value);
                }
                else {
                    _priority.Add(firstType, (int) Priority.Normal);
                }
            }
            if (!_priority.TryGetValue(secondType, out var secondPriority)) {
                var secondAttr = secondType.GetCustomAttribute<PriorityAttribute>(true);
                if (secondAttr != null) {
                    _priority.Add(secondType, secondAttr.Value);
                }
                else {
                    _priority.Add(secondType, (int) Priority.Normal);
                }
            }
            return firstPriority.CompareTo(secondPriority);

        }
    }

    public class SortByPriorityClass<T> : IComparer<ManagedArray<T>.Delegate> {
        
        Dictionary<System.Type, int> _priority = new Dictionary<Type, int>();

        public int Compare(ManagedArray<T>.Delegate a, ManagedArray<T>.Delegate b) {
            if (a == null && b == null) {
                return 0;
            }
            if (a == null) {
                return 1;
            }
            if (b == null) {
                return -1;
            }
            var first = a.GetType();
            var second = b.GetType();
            if (!_priority.TryGetValue(first, out var firstPriority)) {
                var firstAttr = first.GetCustomAttribute<PriorityAttribute>(true);
                if (firstAttr != null) {
                    _priority.Add(first, firstAttr.Value);
                }
                else {
                    _priority.Add(first, (int) Priority.Normal);
                }
            }
            if (!_priority.TryGetValue(second, out var secondPriority)) {
                var secondAttr = second.GetCustomAttribute<PriorityAttribute>(true);
                if (secondAttr != null) {
                    _priority.Add(second, secondAttr.Value);
                }
                else {
                    _priority.Add(second, (int) Priority.Normal);
                }
            }
            return firstPriority.CompareTo(secondPriority);
            
        }
    }
    
    public class SortByPriorityReceiver : IComparer<IReceive> {
        Dictionary<System.Type, int> _priority = new Dictionary<Type, int>();

        public int Compare(IReceive firstR, IReceive secondR) {
            if (firstR == null && secondR == null) {
                return 0;
            }
            if (firstR == null) {
                return 1;
            }
            if (secondR == null) {
                return -1;
            }
            var first = firstR.GetType();
            var second = secondR.GetType();
            if (!_priority.TryGetValue(first, out var firstPriority)) {
                var firstAttr = first.GetCustomAttribute<PriorityAttribute>(true);
                if (firstAttr != null) {
                    _priority.Add(first, firstAttr.Value);
                }
                else {
                    _priority.Add(first, (int) Priority.Normal);
                }
            }
            if (!_priority.TryGetValue(second, out var secondPriority)) {
                var secondAttr = second.GetCustomAttribute<PriorityAttribute>(true);
                if (secondAttr != null) {
                    _priority.Add(second, secondAttr.Value);
                }
                else {
                    _priority.Add(second, (int) Priority.Normal);
                }
            }
            return firstPriority.CompareTo(secondPriority);
        }
    }
}
