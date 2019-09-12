using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class ColliderComponent : IComponent {
        private CachedUnityComponent<Collider> _component;
        public Collider Collider { get { return _component.Component; } }

        public ColliderComponent(Collider collider) {
            _component = new CachedUnityComponent<Collider>(collider);
            LocalCenter = new Vector3(0, Collider.bounds.size.y * 0.5f, 0);
        }

        public Vector3 LocalCenter { get; }

        public ColliderComponent(SerializationInfo info, StreamingContext context) {
            _component = info.GetValue(nameof(_component), _component);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_component), _component);
        }
    }
}
