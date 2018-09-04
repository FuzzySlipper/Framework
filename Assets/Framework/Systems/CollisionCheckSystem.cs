using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CollisionCheckSystem : SystemBase, IMainSystemUpdate, IReceiveGlobal<SphereCastEvent> {

        private RaycastHit[] _rayHits = new RaycastHit[25];

        public void OnSystemUpdate(float dt) {
            var rayForward = EntityController.GetComponentArray<CollisionCheckForward>();
            if (rayForward != null) {
                rayForward.RunAction(CheckRayFwd);
            }
        }

        private void CheckRayFwd(CollisionCheckForward c) {
            var entity = c.GetEntity();
            if (entity == null || !entity.Tags.Contain(EntityTags.CheckingCollision)) {
                return;
            }
            var tr = entity.Get<TransformComponent>().Tr;
            if (tr == null) {
                return;
            }
            var dir = (c.LastPos - tr.position);
            var ray = new Ray(tr.position + tr.forward * c.RayDistance, dir.normalized);
            int limit = Physics.RaycastNonAlloc( ray, _rayHits, dir.magnitude + c.RayDistance,
                LayerMasks.DefaultCollision);
            _rayHits.SortByDistanceAsc(limit);
            CheckRayList(entity, limit);
            c.LastPos = tr.position;
        }

        private void CheckRayList(Entity entity, int limit) {
            for (int i = 0; i < limit; i++) {
                var hitEntity = EntityController.GetEntity(MonoBehaviourToEntity.GetEntityId(_rayHits[i].collider));
                if (hitEntity == null || hitEntity == entity) {
                    continue;
                }
                new CollisionEvent(entity, hitEntity, _rayHits[i].point, _rayHits[i].normal).Post(entity);
            }
        }

        //private void CheckBoxCast() {
        //    var hitLimit = Physics.BoxCastNonAlloc(_lastPos, Owner.Collider.bounds.extents, dir.normalized, _rayHits, Tr.rotation, dir.magnitude, LayerMasks.DefaultCollision);
        //}

        public void HandleGlobal(List<SphereCastEvent> arg) {
            for (int i = 0; i < arg.Count; i++) {
                var rayEvent = arg[i];
                var rayLimit = Physics.SphereCastNonAlloc(rayEvent.Ray, rayEvent.Radius, _rayHits, rayEvent.Distance, LayerMasks.DefaultCollision);
                _rayHits.SortByDistanceAsc(rayLimit);
                CheckRayList(rayEvent.Owner, rayLimit);
            }
        }
    }

    public struct SphereCastEvent : IEntityMessage {
        public Ray Ray;
        public float Radius;
        public float Distance;
        public Entity Owner;

        public SphereCastEvent(Ray ray, float radius, float distance, Entity owner) {
            Ray = ray;
            Radius = radius;
            Distance = distance;
            Owner = owner;
        }
    }
}
