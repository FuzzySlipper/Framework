using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct EntityDetailsChanged : IEntityMessage {
        public Entity Target;

        public EntityDetailsChanged(Entity target) {
            Target = target;
        }
    }
}
