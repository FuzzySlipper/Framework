using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EntityTriggerCollider : EntityIdentifier {

        [SerializeField] private bool _canGenerateCollisions = true;

        void OnTriggerEnter(Collider other) {
            if (!enabled || !_canGenerateCollisions) {
                return;
            }
            var hitEntity = EntityController.GetEntity(UnityToEntityBridge.GetEntityId(other));
            var entity = EntityController.GetEntity(EntityID);
            if (hitEntity == null || hitEntity.Id == EntityID) {
                return;
            }
            if (!entity.Tags.Contain(EntityTags.CanUnityCollide) || !hitEntity.Tags.Contain(EntityTags.CanUnityCollide)) {
                return;
            }
            var sourceNode = entity.FindNode<CollidableNode>();
            var targetNode = hitEntity.FindNode<CollidableNode>();
            if (sourceNode == null || targetNode == null) {
                return;
            }
            var hitPnt = other.ClosestPointOnBounds(transform.position);
#if DEBUG
            DebugExtension.DebugPoint(hitPnt, Color.magenta, 1.5f, 4f);
#endif
            hitEntity.Post(new CollisionEvent(entity, sourceNode, targetNode, hitPnt, (hitPnt - transform.position).normalized));
            entity.Post(new PerformedCollisionEvent(sourceNode, targetNode, hitPnt, (hitPnt - transform.position).normalized));
        }
    }
}
