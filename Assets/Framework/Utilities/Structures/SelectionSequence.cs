using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SelectionSequence<T> {

        private readonly List<T> items = new List<T>();

        private int currentIndex = 0;

        public SelectionSequence(T[] array) {
            this.items.AddRange(array);
            Reset();
        }

        public void Reset() {
            this.currentIndex = 0;
        }

        public void MoveToNext() {
            this.currentIndex = (currentIndex + 1) % this.items.Count;
        }

        public void MoveToPrevious() {
            int decremented = this.currentIndex - 1;
            this.currentIndex = decremented < 0 ? this.items.Count - 1 : decremented;
        }

        public T Current {
            get {
                return this.items[this.currentIndex];
            }
        }

        public void Select(int index) {
            this.currentIndex = index;
        }

        public void Select(T item) {
            for (int i = 0; i < this.items.Count; ++i) {
                if (this.items[i].Equals(item)) {
                    Select(i);
                    return;
                }
            }

            throw new Exception("Can't find the item to select: " + item.ToString());
        }

        public int Count {
            get {
                return this.items.Count;
            }
        }

        public T GetAt(int index) {
            return this.items[index];
        }

    }
}
