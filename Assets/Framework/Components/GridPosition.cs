using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public class GridPosition : IComponent {
        public Point3 Value;

        public GridPosition(Point3 value) {
            Value = value;
<<<<<<< HEAD
=======
        }

        public GridPosition() {
>>>>>>> FirstPersonAction
        }

        public static implicit operator Point3(GridPosition reference) {
            return reference.Value;
        }

        public GridPosition(SerializationInfo info, StreamingContext context) {
            Value = (Point3) info.GetValue(nameof(Value), typeof(Point3));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Value), Value);
        }
    }
}
