using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct AnimationEventTriggered : IEntityMessage {
        public string Event { get; }
        public Entity Entity { get; }

        public AnimationEventTriggered(Entity entity, string eventName) {
            Event = eventName;
            Entity = entity;
        }
    }
}
