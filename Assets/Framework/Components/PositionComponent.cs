using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {
    [System.Serializable]
    public class PositionComponent : ComponentBase {

        [SerializeField] private Float3 _position;

        private CachedComponent<ColliderComponent> _collider;
        private Transform Tr { get { return Entity.Tr; } }
        
        protected override void SetEntity(Entity entity) {
            base.SetEntity(entity);
            if (entity != null) {
                _collider = new CachedComponent<ColliderComponent>(entity);
            }
        }

        public Vector3 Position { get { return (Tr != null ? Tr.position : _position.toVector3()) + (_collider?.c?.LocalCenter ?? Vector3.zero); } }
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
    }
}
