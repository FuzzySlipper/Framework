using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public class RotationComponent : IComponent {
        [SerializeField] private Float4 _rotation;
        [SerializeField] private int _owner;
        private CachedComponent<TransformComponent> _cached;

        public int Owner {
            get { return _owner; }
            set {
                if (_owner == value) {
                    return;
                }
                _owner = value;
                if (_owner < 0) {
                    return;
                }
                _cached = new CachedComponent<TransformComponent>(this.GetEntity());
            }
        }

        public Quaternion Rotation {
            get { return _cached.c.Tr != null ? _cached.c.Tr.rotation : _rotation.toQuaternion(); }
        }

        public RotationComponent() {}

        public RotationComponent(Quaternion rotation) {
            _rotation = rotation;
        }
    }
}
