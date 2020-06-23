using System;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class SystemBase : IDisposable {

        public virtual void Dispose() {
            World.RemoveSystem(this);
        }
    }

    public abstract class SystemBase<T> : SystemBase where T : SystemBase {
        
        private static T _main;

        public static T Get {
            get {
                if (_main == null) {
                    _main = World.Get<T>();
                }
                return _main;
            }
        }
    }

    public abstract class SystemWithSingleton<T, TV> : SystemBase<T> where T : SystemWithSingleton<T,TV> {
        
        protected static readonly List<TV> SingletonList = new List<TV>();
        public static TV Current { get; protected set; }

        public static void Set(TV component) {
            if (component == null) {
                return;
            }
            if (!SingletonList.Contains(component)) {
                SingletonList.Add(component);
            }
            Get.SetCurrent(component);
        }

        public static void Remove(TV component) {
            SingletonList.Remove(component);
            if (Current.Equals(component)) {
                Get.SetCurrent(SingletonList.LastElement());
            }
        }

        public static void RemoveCurrent() {
            if (Current == null) {
                return;
            }
            SingletonList.Remove(Current);
            Get.SetCurrent(SingletonList.LastElement());
        }

        protected virtual void SetCurrent(TV current) {
            Current = current;
        }
    }
}