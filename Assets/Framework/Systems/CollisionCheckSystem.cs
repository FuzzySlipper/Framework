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

        private NodeList<CollisionCheckForwardNode> _list;
        

        public CollisionCheckSystem() {
            NodeFilter<CollisionCheckForwardNode>.Setup(CollisionCheckForwardNode.GetTypes());
            _list = EntityController.GetNodeList<CollisionCheckForwardNode>();
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            _list.Run(UpdateNode);
        }

        private void UpdateNode(ref CollisionCheckForwardNode node) {
            var tr = node.Tr;
            if (tr == null) {
                node.Forward.LastPos = null;
                return;
            }
            if (Raycast(node.Entity, new Ray(tr.position, tr.forward), node.Forward.RayDistance, false) != null) {
                node.Forward.LastPos = tr.position;
                return;
            }
            if (node.Forward.LastPos != null) {
                var backwardDir = (tr.position - node.Forward.LastPos.Value);
                Raycast(node.Entity, new Ray(node.Forward.LastPos.Value, backwardDir.normalized), backwardDir.magnitude, false);
            }
            node.Forward.LastPos = tr.position;
        }

        public static CollisionEvent? Raycast(Entity entity, Ray ray, float distance, bool limitCollision) {
#if DEBUG_RAYCAST
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * distance, Color.cyan, 2.5f);
#endif
            int limit = Physics.RaycastNonAlloc( ray, _rayHits, distance, LayerMasks.DefaultCollision);
            _rayHits.SortByDistanceAsc(limit);
            return CheckRayList(entity, limit, limitCollision);
        }

        public static CollisionEvent? SphereCast(Entity entity, Ray ray, float distance, float radius, bool limitCollision) {
#if DEBUG_RAYCAST
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * distance, Color.cyan, 2.5f);
#endif
            var limit = Physics.SphereCastNonAlloc(ray, radius, _rayHits, distance, LayerMasks.DefaultCollision);
            _rayHits.SortByDistanceAsc(limit);
            return CheckRayList(entity, limit, limitCollision);
        }

        public static void OverlapSphere(Entity entity, Entity ignoreEntity, Vector3 position, float radius, bool limitEnemy) {
#if DEBUG_RAYCAST
            DebugExtension.DebugCircle(position, Color.red, radius, 2.5f);
#endif
            var limit = Physics.OverlapSphereNonAlloc(position, radius, _colliders, LayerMasks.DefaultCollision);
            CheckColliderList(entity, ignoreEntity, position, limit, limitEnemy);
        }
        

        private static CollisionEvent? CheckRayList(Entity originEntity, int limit, bool limitEnemy) {
            for (int i = 0; i < limit; i++) {
                if (originEntity.IsDestroyed()) {
                    return null;
                }
                var hit = _rayHits[i];
                
                Entity hitEntity = EntityController.GetEntity(UnityToEntityBridge.GetEntityId(hit.collider));
                bool isEnvironment = hitEntity == null && hit.transform.IsEnvironment();
                
#if DEBUG_RAYCAST
                    Color pointColor = Color.white;
                    if (isEnvironment) {
                        pointColor = Color.green;
                    }
                    else if (hit.transform.CompareTag(StringConst.TagInvalidCollider)) {
                        pointColor = Color.magenta;
                    }
                    else if (hitEntity != null) {
                        pointColor = Color.red;
                    }
                    DebugExtension.DebugPoint(_rayHits[i].point + (Vector3.up * (i * 0.1f)), pointColor, 0.25f, 2.5f);
#endif
                if (isEnvironment) {
#if DEBUG
                    DebugLog.Add(originEntity.DebugId + " hit environment " + _rayHits[i].transform.name);
#endif
                    originEntity.Post(new EnvironmentCollisionEvent(originEntity, _rayHits[i].point, _rayHits[i].normal));
                    return null;
                }
                if (hitEntity == null) {
                    continue;
                }
                if (IsValidCollision(originEntity, limitEnemy, hitEntity, _rayHits[i].collider, out var sourceNode, out var targetNode)) {
                    var ce = new CollisionEvent(originEntity, sourceNode, targetNode, _rayHits[i].point, _rayHits[i].normal);
                    hitEntity.Post(ce);
                    originEntity.Post(new PerformedCollisionEvent(sourceNode, targetNode, _rayHits[i].point, _rayHits[i].normal));
                    return ce;
                }
            }
            return null;
        }

        private static bool IsValidCollision(Entity entity, bool limitEnemy, Entity hitEntity, Collider collider, out CollidableNode 
        sourceNode, out CollidableNode targetNode) {
            sourceNode = targetNode = null;
            if (hitEntity == entity) {
                return false;
            }
            var tr = collider.transform;
            if (tr.CompareTag(StringConst.TagInvalidCollider) || 
                tr.CompareTag(StringConst.TagSensor)) {
                return false;
            }
            if (!hitEntity.Tags.Contain(EntityTags.CanUnityCollide)) {
                return false;
            }
            if (AreEntitiesConnected(hitEntity, entity)) {
                return false;
            }
#if DEBUG
            DebugLog.Add(entity.DebugId + " hit actor " + tr.name);
#endif
            sourceNode = entity.FindNode<CollidableNode>();
            targetNode = hitEntity.FindNode<CollidableNode>();
            if (sourceNode == null || targetNode == null || sourceNode == targetNode) {
                return false;
            }
            if (limitEnemy) {
                if (World.Get<FactionSystem>().AreEnemies(sourceNode.Entity, targetNode.Entity)) {
                    if (entity.Tags.IsConfused) {
                        return false;
                    }
                }
                else {
                    if (!entity.Tags.IsConfused) {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void CheckColliderList(Entity originEntity, Entity ignoreEntity,  Vector3 position, int limit, bool limitEnemy) {
            for (int i = 0; i < limit; i++) {
                if (originEntity.IsDestroyed()) {
                    return;
                }
                var collider = _colliders[i];
                var hitEntity = EntityController.GetEntity(UnityToEntityBridge.GetEntityId(collider));
                if (hitEntity == ignoreEntity || hitEntity == null) {
                    continue;
                }
                if (IsValidCollision(originEntity, limitEnemy , hitEntity, _rayHits[i].collider, out var sourceNode, out var targetNode)) {
                    CollisionExtensions.GenerateHitLocDir(position, hitEntity, collider, out var hitPnt, out var hitNormal);
                    var ce = new CollisionEvent(originEntity, sourceNode, targetNode, hitPnt, hitNormal);
                    hitEntity.Post(ce);
                    originEntity.Post(new PerformedCollisionEvent(sourceNode, targetNode, hitPnt, hitNormal));
                }
            }
        }

        private static bool AreEntitiesConnected(Entity entity, Entity hitEntity) {
            if (AreEntitiesParentedOrConnected(entity, hitEntity)) {
                return true;
            }
            if (entity.ParentId >= 0 && CheckRootConnected(entity, hitEntity)) {
                return true;
            }
            if (hitEntity.ParentId >= 0 && CheckRootConnected(hitEntity, entity)) {
                return true;
            }
            return false;
        }

        private static bool CheckRootConnected(Entity entity, Entity other) {
            var entParent = entity.GetParent();
            while (entParent != null) {
                if (AreEntitiesParentedOrConnected(entParent, other)) {
                    return true;
                }
                entParent = entParent.GetParent();
            }
            return false;
        }

        private static bool AreEntitiesParentedOrConnected(Entity entity, Entity other) {
            if (other == entity || other.ParentId == entity.Id || entity.ParentId == other.Id) {
                return true;
            }
            if (entity.ParentId >= 0 && other.ParentId == entity.ParentId) {
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

    public class CollisionCheckForwardNode : BaseNode {

        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<ColliderComponent> _collider = new CachedComponent<ColliderComponent>();
        private CachedComponent<CollisionCheckForward> _forward = new CachedComponent<CollisionCheckForward>();

        public TransformComponent Tr { get => _tr.Value; }
        public Collider Collider { get => _collider.Value.Collider; }
        public CollisionCheckForward Forward => _forward.Value;
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _tr, _collider, _forward
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(TransformComponent),
                typeof(CollisionCheckForward),
            };
        }
    }
}
