using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EntityPhysicsCollider : EntityIdentifier {

        void OnCollisionEnter(Collision collision) {
            if (!enabled) {
                return;
            }
            var hitEntity = EntityController.GetEntity(MonoBehaviourToEntity.GetEntityId(collision.collider));
            var entity = EntityController.GetEntity(EntityID);
            if (hitEntity == null || hitEntity == entity) {
                return;
            }
            if (!entity.Tags.Contain(EntityTags.CanUnityCollide) || !hitEntity.Tags.Contain(EntityTags.CanUnityCollide)) {
                return;
            }
            var collisionPnt = collision.contacts[0];
            entity.Post(new CollisionEvent(entity, hitEntity, collisionPnt.point, collisionPnt.normal));
        }
    }
}
