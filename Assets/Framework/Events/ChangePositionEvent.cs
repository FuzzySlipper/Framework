using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct ChangePositionEvent : IEntityMessage {

        public Vector3 Position { get; }
        public Quaternion? Rotation { get;}

        public ChangePositionEvent(Vector3 position) {
            Position = position;
            Rotation = null;
        }
        public ChangePositionEvent(Vector3 position, Quaternion rotation) {
            Position = position;
            Rotation = rotation;
        }
    }
}
