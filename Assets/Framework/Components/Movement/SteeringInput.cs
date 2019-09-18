using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {
    [System.Serializable]
    public sealed class SteeringInput : IComponent {

        public Vector3 Move;
        public Vector3 Look;
        
        public SteeringInput() {}

        public SteeringInput(SerializationInfo info, StreamingContext context) {
            Move = info.GetValue(nameof(Move), Move);
            Look = info.GetValue(nameof(Look), Look);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Move), Move);
            info.AddValue(nameof(Look), Look);
        }

        public void Reset() {
            Move = Look = Vector3.zero;
        }

        public void Set(Vector3 move, Vector3 look) {
            Move = move;
            Look = look;
        }
    }
}