using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EntityPhysicsCollider : EntityIdentifier {

        [SerializeField] private bool _canGenerateCollisions = true;
        [SerializeField] private bool _limitToEnemy = false;

        private HashSet<Collider> _checkedColliders = new HashSet<Collider>();

        public void OnPoolSpawned() {
            _checkedColliders.Clear();
        }

        public void OnPoolDespawned() {
        }

        void OnCollisionEnter(Collision collision) {
            if (!enabled || !_canGenerateCollisions || _checkedColliders.Contains(collision.collider)) {
                return;
            }
            var other = collision.collider;
            _checkedColliders.Add(other);
            var hitEntity = EntityController.GetEntity(UnityToEntityBridge.GetEntityId(other));
            var entity = EntityController.GetEntity(EntityID);
            if (hitEntity == null || hitEntity.Id == EntityID) {
                return;
            }
            if (!CollisionCheckSystem.IsValidCollision(entity, _limitToEnemy, hitEntity, other, out var sourceNode, out var targetNode)) {
                return;
            }
            var collisionPnt = collision.contacts[0];
            var hitPnt = collisionPnt.point;
            var hitNormal = collisionPnt.normal;
#if DEBUG
            DebugExtension.DebugPoint(hitPnt, Color.magenta, 1.5f, 4f);
#endif
            hitEntity.Post(new CollisionEvent(entity, sourceNode, targetNode, hitPnt, hitNormal));
            entity.Post(new PerformedCollisionEvent(sourceNode, targetNode, hitPnt, hitNormal));
        }
    }
}
