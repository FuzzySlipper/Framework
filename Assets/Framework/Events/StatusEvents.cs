using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public struct TagTimerEvent {

        public Entity Entity { get; }
        public float TimeEnd { get; }
        public int Tag { get; }

        public TagTimerEvent(Entity entity, float timeEnd, int tag) {
            Entity = entity;
            TimeEnd = timeEnd;
            Tag = tag;
        }
    }

    public struct ConfusionEvent : IEntityMessage {
        public float Length { get; }
        public Entity Entity { get; }
        public bool Active { get; }

        public ConfusionEvent(Entity entity, float length, bool active) {
            Length = length;
            Entity = entity;
            Active = active;
        }
    }

    public struct TagChangeEvent : IEntityMessage {
        public Entity Entity { get; }
        public bool Active { get; }
        public int Tag { get; }

        public TagChangeEvent(Entity entity, int tag, bool active) {
            Tag = tag;
            Entity = entity;
            Active = active;
        }
    }
}
