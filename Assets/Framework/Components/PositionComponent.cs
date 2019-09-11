using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {
    [System.Serializable]
    public class PositionComponent : ComponentBase {

        [SerializeField] private Float3 _position;

        private Transform Tr { get { return Entity.Tr; } }
        
        public Vector3 Position { get { return (Tr != null ? Tr.position : _position.toVector3()); } }
        public Float3 PositionF3 { get { return Tr != null ? new Float3(Tr.position) : _position; }}

        public PositionComponent(Float3 value) {
            _position = value;
        }

        public PositionComponent(){}

        public static implicit operator Float3(PositionComponent reference) {
            return reference.PositionF3;
        }

        public static implicit operator Vector3(PositionComponent reference) {
            return reference.Position;
        }

        public PositionComponent(SerializationInfo info, StreamingContext context) {
            _position = info.GetValue(nameof(_position), _position);
        }
        
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_position), _position);
        }
    }
}
