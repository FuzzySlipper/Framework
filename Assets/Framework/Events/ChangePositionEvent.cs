using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct ChangePositionEvent : IEntityMessage {

        public Vector3 Position { get; }
        public Quaternion? Rotation { get;}
        public Entity Target { get; }
        public ChangePositionEvent(Entity target, Vector3 position) {
            Target = target;
            Position = position;
            Rotation = null;
        }
        public ChangePositionEvent(Entity target, Vector3 position, Quaternion rotation) {
            Target = target;
            Position = position;
            Rotation = rotation;
        }
    }
}
