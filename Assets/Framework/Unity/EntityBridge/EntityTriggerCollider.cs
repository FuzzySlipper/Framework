using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PixelComrades {
    public class EntityTriggerCollider : EntityIdentifier, IPoolEvents {

        [SerializeField] private bool _canGenerateCollisions = true;
        [SerializeField] private bool _limitToEnemy = false;

        private HashSet<Collider> _checkedColliders = new HashSet<Collider>();
        
        public void OnPoolSpawned() {
            _checkedColliders.Clear();
        }

        public void OnPoolDespawned() {}
        
        void OnTriggerEnter(Collider other) {
            if (!enabled || !_canGenerateCollisions || _checkedColliders.Contains(other)) {
                return;
            }
            _checkedColliders.Add(other);
            var hitEntity = EntityController.GetEntity(UnityToEntityBridge.GetEntityId(other));
            var entity = EntityController.GetEntity(EntityID);
            if (hitEntity == null || hitEntity.Id == EntityID) {
                return;
            }
            if (!CollisionCheckSystem.IsValidCollision(entity, _limitToEnemy, hitEntity, other, out var sourceNode, out var targetNode)) {
                return;
            }
            var position = transform.position;
            var hitPnt = other.ClosestPointOnBounds(position);
            var hitNormal = (hitPnt - position).normalized;
#if DEBUG
            DebugExtension.DrawPoint(hitPnt, Color.yellow, 1.5f, 4f);
#endif
            hitEntity.Post(new CollisionEvent(entity, sourceNode, targetNode, hitPnt, hitNormal));
            entity.Post(new PerformedCollisionEvent(sourceNode, targetNode, hitPnt, hitNormal));
        }

        
    }
}
