using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class CollisionExtensions {
        public static void GenerateHitLocDir(TransformComponent origin, TransformComponent target, Collider targetCollider, 
            out Vector3 hitPnt, out Vector3 normal) {
            if (origin == target) {
                hitPnt = target.position;
                normal = target.forward;
                return;
            }
            var originPos = origin.position;
            if (targetCollider != null) {
                //hitPnt += new Vector3(0, collider.Collider.bounds.size.y * 0.5f, 0);
                hitPnt = targetCollider.ClosestPointOnBounds(originPos);
            }
            else {
                hitPnt = target.position;
            }
            normal = (originPos - hitPnt).normalized;
            //if (target == origin) {
            //    hitPnt += dir * 1;
            //}
            //else if (collider != null && collider.Collider != null) {
            //    //dir = hitPnt - Owner.Tr.position;
            //    //dir = dir.normalized;
            //    //hitPnt += (Owner.WorldCenter - hitPnt).normalized * targetActor.Collider.bounds.size.z * 0.5f;
            //    hitPnt += dir * collider.Collider.bounds.size.z * 0.5f;
            //}
        }

        public static void GenerateHitLocDir(Vector3 origin, Entity target, Collider collider, out Vector3 hitPnt, out Vector3 
        normal) {
            if (collider != null && collider != null) {
                hitPnt = collider.ClosestPointOnBounds(origin);
            }
            else {
                hitPnt = target.GetPosition();
            }
            normal = (origin - hitPnt).normalized;
        }

        public static float GetHitMultiplier(CollisionResult hit, CharacterTemplate entity) {
            var multi = 1f;
            if (hit == CollisionResult.CriticalHit) {
                var critStat = entity.Stats.Get(Stat.CriticalMulti);
                if (critStat != null) {
                    multi = critStat.Value;
                }
            }
            if (hit == CollisionResult.Graze) {
                multi = 0.5f;
            }
            return multi;
        }

        public static Vector3 MissPosition(CommandTarget target) {
            if (target.Target != null) {
                var collider = target.Target.Get<ColliderComponent>().Collider;
                if (collider != null) {
                    return target.GetPosition + (Random.onUnitSphere * collider.bounds.size.z * 0.5f);
                }
            }
            return target.GetPosition + (Random.onUnitSphere * 1.5f);
        }
    }
}
