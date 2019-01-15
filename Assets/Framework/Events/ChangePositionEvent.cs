using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct ChangePositionEvent : IEntityMessage {

        public Vector3 Position { get; }

        public ChangePositionEvent(Vector3 position) {
            Position = position;
        }
    }
}
