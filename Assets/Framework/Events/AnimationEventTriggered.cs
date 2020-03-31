using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct AnimationEventTriggered : IEntityMessage {
        public AnimationEvent Event { get; }
        public Entity Entity { get; }

        public AnimationEventTriggered(Entity entity, AnimationEvent eventName) {
            Event = eventName;
            Entity = entity;
        }
    }
}
