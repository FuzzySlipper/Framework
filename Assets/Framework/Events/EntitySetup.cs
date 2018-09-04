using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct EntitySetup : IEntityMessage {
        public Entity Entity { get; }

        public EntitySetup(Entity entity) {
            Entity = entity;
        }
    }
}
