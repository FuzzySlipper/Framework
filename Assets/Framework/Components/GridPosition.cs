using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public struct GridPosition : IComponent {
        public Point3 Position;

        public GridPosition(Point3 position) : this() {
            Position = position;
        }

        public static implicit operator Point3(GridPosition reference) {
            return reference.Position;
        }

        public GridPosition(SerializationInfo info, StreamingContext context) {
            Position = (Point3) info.GetValue(nameof(Position), typeof(Point3));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Position), Position);
        }
    }
}
