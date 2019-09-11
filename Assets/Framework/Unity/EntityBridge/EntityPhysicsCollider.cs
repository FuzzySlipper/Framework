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
            var impacts = entity.Get<ActionImpacts>();
            var collisionPnt = collision.contacts[0];
#if DEBUG
            DebugExtension.DebugPoint(collisionPnt.point, Color.magenta, 1.5f, 4f);
#endif
            hitEntity.Post(new CollisionEvent(entity, hitEntity, collisionPnt.point, collisionPnt.normal, impacts));
            entity.Post(new PerformedCollisionEvent(entity, hitEntity, collisionPnt.point, collisionPnt.normal, impacts));
        }
    }
}
