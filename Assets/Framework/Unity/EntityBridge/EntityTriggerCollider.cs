using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EntityTriggerCollider : EntityIdentifier {
        void OnTriggerEnter(Collider other) {
            if (!enabled) {
                return;
            }
            var hitEntity = EntityController.GetEntity(MonoBehaviourToEntity.GetEntityId(other));
            var entity = EntityController.GetEntity(Entity);
            if (hitEntity == null || hitEntity == entity) {
                return;
            }
            if (!entity.Tags.Contain(EntityTags.CanUnityCollide) || !hitEntity.Tags.Contain(EntityTags.CanUnityCollide)) {
                return;
            }
            var hitPnt = other.ClosestPointOnBounds(transform.position);
            new CollisionEvent(entity, hitEntity, hitPnt, (hitPnt - transform.position).normalized).Post(entity);
        }
    }
}
