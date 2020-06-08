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
}