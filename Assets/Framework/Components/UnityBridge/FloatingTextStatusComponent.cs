using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class FloatingTextStatusComponent : IComponent {
        private CachedTransform _component;

        public Transform Tr { get { return _component.Tr; } }
        public Vector3 Offset { get; }

        public FloatingTextStatusComponent(Transform tr, Vector3 offset) {
            _component = new CachedTransform(tr);
            Offset = offset;
        }

        public FloatingTextStatusComponent(SerializationInfo info, StreamingContext context) {
            _component = info.GetValue(nameof(_component), _component);
            Offset = info.GetValue(nameof(Offset), Offset);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_component), _component);
            info.AddValue(nameof(Offset), Offset);
        }
    }
}
