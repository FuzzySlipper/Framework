using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class FlightMoveInput : IComponent {
        
        public Vector3 LookInputVector { get; set; }
        public Vector3 MoveInputVector { get; set; }

        public bool CanMove = true;

        public void UpdateControl(FlightControl control) {
            control.Thrust = MoveInputVector.z;
            control.Pitch = LookInputVector.y;
            control.Yaw = LookInputVector.x;
            control.StrafeHorizontal = MoveInputVector.x;
            control.StrafeVertical = MoveInputVector.y;
        }

        public FlightMoveInput() {}

        public FlightMoveInput(SerializationInfo info, StreamingContext context) {
            CanMove = info.GetValue(nameof(CanMove), CanMove);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(CanMove), CanMove);
        }
    }
}
