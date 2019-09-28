using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EntityPhysicsCollider : EntityIdentifier {

        [SerializeField] private bool _canGenerateCollisions = true;

        void OnCollisionEnter(Collision collision) {
            if (!enabled || !_canGenerateCollisions) {
                return;
            }
            var hitEntity = EntityController.GetEntity(UnityToEntityBridge.GetEntityId(collision.collider));
            var entity = EntityController.GetEntity(EntityID);
            if (hitEntity == null || hitEntity == entity) {
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
            var collisionPnt = collision.contacts[0];
#if DEBUG
            DebugExtension.DebugPoint(collisionPnt.point, Color.magenta, 1.5f, 4f);
#endif
            hitEntity.Post(new CollisionEvent(entity, sourceNode, targetNode, collisionPnt.point, collisionPnt.normal));
            entity.Post(new PerformedCollisionEvent(sourceNode, targetNode, collisionPnt.point, collisionPnt.normal));
        }
    }
}
