using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Serializable]
    public class DotParam {
        public float Time;
    }

    public class DotParamsList<T> : SortedList<float, T> {

        public DotParamsList(int capacity) : base(capacity) {
        }

        private static int BinarySearch<TV>(IList<TV> list, TV value) {
            if (list == null) {
                throw new ArgumentNullException("list");
            }

            var comp = Comparer<TV>.Default;
            int lo = 0, hi = list.Count - 1;
            while (lo < hi) {
                var m = (hi + lo) / 2; // this might overflow; be careful.
                if (comp.Compare(list[m], value) < 0) {
                    lo = m + 1;
                }
                else {
                    hi = m - 1;
                }
            }
            if (comp.Compare(list[lo], value) < 0) {
                lo++;
            }

            return lo;
        }

        public int FindIndexPerTime(float time) {
            return BinarySearch(Keys, time);
        }
    }

    [Serializable]
    public abstract class SortedParamsList<T> where T : DotParam {
        protected DotParamsList<T> SortedParams;
        public T[] Params = new T[0];

        public void Init() {
            SortedParams = new DotParamsList<T>(Params.Length);
            foreach (var param in Params) {
                SortedParams[param.Time] = param;
            }
        }

        public void Update() {
            if (SortedParams == null) {
                SortedParams = new DotParamsList<T>(Params.Length);
            }
            else {
                SortedParams.Clear();
            }
            for (var i = 0; i < Params.Length; i++) {
                var param = Params[i];
                SortedParams[param.Time] = param;
            }
        }
    }
}
