using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct EntityDisposed : IEntityMessage {
        public Entity Target;

        public EntityDisposed(Entity target) {
            Target = target;
        }
    }
}
