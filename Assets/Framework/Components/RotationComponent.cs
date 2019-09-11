using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class RotationComponent : ComponentBase {

        [SerializeField] private Float4 _rotation;

        public Quaternion Rotation {
            get { return Entity?.Tr != null ? Entity.Tr.rotation : _rotation.toQuaternion(); }
        }

        public RotationComponent() {}

        public RotationComponent(Quaternion rotation) {
            _rotation = rotation;
        }

        public RotationComponent(SerializationInfo info, StreamingContext context) {
            _rotation = info.GetValue(nameof(_rotation), _rotation);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_rotation), _rotation);
        }
    }
}
