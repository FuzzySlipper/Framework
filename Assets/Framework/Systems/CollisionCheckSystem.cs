#define DEBUG_RAYCAST
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace PixelComrades {
    [Priority(Priority.Higher)]
    public class CollisionCheckSystem : SystemBase, IMainSystemUpdate, IReceiveGlobal<SphereCastEvent> {

        private static RaycastHit[] _rayHits = new RaycastHit[25];
        private static Collider[] _colliders = new Collider[25];

        private ManagedArray<CollisionCheckForward> _list;
        private ManagedArray<CollisionCheckForward>.Delegate _del;

        public CollisionCheckSystem() {
            _del = CheckRayFwd;
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            if (_list == null) {
                _list = EntityController.GetComponentArray<CollisionCheckForward>();
            }
            if (_list != null) {
                _list.Run(_del);
            }
        }

        private void CheckRayFwd(CollisionCheckForward c) {
            var entity = c.GetEntity();
            if (entity == null) {
                return;
            }
            if (!entity.Tr) {
                c.LastPos = null;
                return;
            }
            var tr = entity.Tr;
            if (Raycast(entity, new Ray(tr.position, tr.forward), c.RayDistance, false) != null) {
                c.LastPos = tr.position;
                return;
            }
            if (c.LastPos != null) {
                var backwardDir = (tr.position - c.LastPos.Value);
                Raycast(entity, new Ray(c.LastPos.Value, backwardDir.normalized), backwardDir.magnitude, false);
            }
            c.LastPos = tr.position;
        }

        public static CollisionEvent? Raycast(Entity entity, Ray ray, float distance, bool limitCollision, List<IActionImpact> impacts = null) {
#if DEBUG_RAYCAST
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * distance, Color.cyan, 2.5f);
#endif
            int limit = Physics.RaycastNonAlloc( ray, _rayHits, distance, LayerMasks.DefaultCollision);
            _rayHits.SortByDistanceAsc(limit);
            return CheckRayList(entity, limit, limitCollision, impacts);
        }

        public static CollisionEvent? SphereCast(Entity entity, Ray ray, float distance, float radius, bool limitCollision, List<IActionImpact> impacts = null) {
#if DEBUG_RAYCAST
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * distance, Color.cyan, 2.5f);
#endif
            var limit = Physics.SphereCastNonAlloc(ray, radius, _rayHits, distance, LayerMasks.DefaultCollision);
            _rayHits.SortByDistanceAsc(limit);
            return CheckRayList(entity, limit, limitCollision, impacts);
        }

        public static void OverlapSphere(Entity entity, Entity ignoreEntity, Vector3 position, float radius, List<IActionImpact> impacts = null) {
#if DEBUG_RAYCAST
            DebugExtension.DebugCircle(position, Color.red, radius, 2.5f);
#endif
            var limit = Physics.OverlapSphereNonAlloc(position, radius, _colliders, LayerMasks.DefaultCollision);
            CheckColliderList(entity, ignoreEntity, position, limit, impacts);
        }
        

        private static CollisionEvent? CheckRayList(Entity entity, int limit, bool limitEnemy,  List<IActionImpact> impacts) {
            int hitIdex = 0;
            for (int i = 0; i < limit; i++) {
                if (entity.IsDestroyed()) {
                    return null;
                }
                var hit = _rayHits[i];
                var hitEntity = EntityController.GetEntity(UnityToEntityBridge.GetEntityId(_rayHits[i].collider));
#if DEBUG_RAYCAST
                Color pointColor = Color.white;
                if (hitEntity == null) {
                    if (_rayHits[i].transform.CompareTag(StringConst.TagEnvironment)) {
                        pointColor = Color.green;
                    }
                }
                if (hit.transform.CompareTag(StringConst.TagInvalidCollider)) {
                    pointColor = Color.magenta;
                }
                else if (hitEntity == entity) {
                    pointColor = Color.blue;
                }
                else if (hitEntity != null) {
                    pointColor = Color.red;
                }
                DebugExtension.DebugPoint(_rayHits[i].point + (Vector3.up * hitIdex), pointColor, 0.5f, 2.5f );
#endif
                if (hit.transform.CompareTag(StringConst.TagInvalidCollider) || 
                    hit.transform.CompareTag(StringConst.TagSensor)) {
                    continue;
                }
                if (hitEntity == null) {
                    if (_rayHits[i].transform.CompareTag(StringConst.TagEnvironment) || _rayHits[i].transform.gameObject.layer == LayerMasks.NumberWall || _rayHits[i].transform.gameObject.layer == LayerMasks.NumberFloor || _rayHits[i].transform.gameObject.layer == LayerMasks.NumberCeiling) {
#if DEBUG
                        DebugLog.Add(entity.DebugId + " hit environment " + _rayHits[i].transform.name);
#endif
                        entity.Post(new EnvironmentCollisionEvent(entity, _rayHits[i].point, _rayHits[i].normal));
                        return null;
                    }
                    continue;
                }
                if (hitEntity == entity || !hitEntity.Tags.Contain(EntityTags.CanUnityCollide) || hitEntity.ParentId == entity.Id) {
                    continue;
                }
                if ((entity.ParentId >= 0) && (hitEntity.Id == entity.ParentId || hitEntity.ParentId == entity.ParentId)) {
                    continue;
                }
#if DEBUG
                DebugLog.Add(entity.DebugId + " hit actor " + _rayHits[i].transform.name);
#endif
                if (limitEnemy) {
                    if (World.Get<FactionSystem>().AreEnemies(entity, hitEntity)) {
                        if (entity.Tags.IsConfused) {
                            continue;
                        }
                    }
                    else {
                        if (!entity.Tags.IsConfused) {
                            continue;
                        }
                    }
                }
                if (impacts == null) {
                    impacts = entity.Get<ActionImpacts>()?.Impacts;
                }
                var ce = new CollisionEvent(entity, hitEntity, _rayHits[i].point, _rayHits[i].normal, impacts);
                hitEntity.Post(ce);
                entity.Post(new PerformedCollisionEvent(entity, hitEntity, _rayHits[i].point, _rayHits[i].normal, impacts));
                return ce;
            }
            return null;
        }

        private static void CheckColliderList(Entity entity, Entity ignoreEntity,  Vector3 position, int limit, List<IActionImpact> impacts) {
            for (int i = 0; i < limit; i++) {
                if (entity.IsDestroyed()) {
                    return;
                }
                var collider = _colliders[i];
                var hitEntity = EntityController.GetEntity(UnityToEntityBridge.GetEntityId(collider));
                if (collider.transform.CompareTag(StringConst.TagInvalidCollider) || 
                    collider.transform.CompareTag(StringConst.TagSensor)) {
                    continue;
                }
                if (hitEntity == null || !hitEntity.Tags.Contain(EntityTags.CanUnityCollide)) {
                    continue;
                }
                if (AreEntitiesConnected(entity, hitEntity) || AreEntitiesConnected(ignoreEntity, hitEntity)) {
                    continue;
                }
#if DEBUG
                DebugLog.Add(entity.DebugId + " hit actor " + collider.transform.name);
#endif
                if (impacts == null) {
                    impacts = entity.Get<ActionImpacts>()?.Impacts;
                }
                CollisionExtensions.GenerateHitLocDir(position, hitEntity, out var hitPnt, out var normal);
                var ce = new CollisionEvent(entity, hitEntity, hitPnt, normal, impacts);
                hitEntity.Post(ce);
                entity.Post(new PerformedCollisionEvent(entity, hitEntity, hitPnt, normal, impacts));
            }
        }

        private static bool AreEntitiesConnected(Entity entity, Entity hitEntity) {
            if (hitEntity == entity || hitEntity.ParentId == entity.Id || (entity.ParentId >= 0) && (hitEntity.Id == entity.ParentId || hitEntity.ParentId == entity.ParentId)) {
                return true;
            }
            return false;
        }

        //private void CheckBoxCast() {
        //    var hitLimit = Physics.BoxCastNonAlloc(_lastPos, Owner.Collider.bounds.extents, dir.normalized, _rayHits, Tr.rotation, dir.magnitude, LayerMasks.DefaultCollision);
        //}

        public void HandleGlobal(SphereCastEvent rayEvent) {
            SphereCast(rayEvent.Owner, rayEvent.Ray, rayEvent.Distance, rayEvent.Radius, false);
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
