using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {
    [System.Serializable]
    public class PositionComponent : IComponent {
        [SerializeField] public Float3 _position;
        [SerializeField] private int _owner = -1;
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

        public Vector3 Position { get { return _cached.c.Tr != null ? _cached.c.Tr.position : _position.toVector3(); } }

        public PositionComponent(Float3 value) {
            _position = value;
        }

        public PositionComponent(){}

        public static implicit operator Float3(PositionComponent reference) {
            return reference._position;
        }

        public static implicit operator Vector3(PositionComponent reference) {
            return reference.Position;
        }
    }
}
