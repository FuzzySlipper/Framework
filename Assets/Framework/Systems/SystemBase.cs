using System;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class SystemBase : IDisposable {

        public virtual void Dispose() {
            World.RemoveSystem(this);
        }
    }
}