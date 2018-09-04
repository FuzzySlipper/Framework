using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class CollisionExtensions {
        public static void GenerateHitLocDir(Entity origin, Entity target, out Vector3 hitPnt, out Vector3 dir) {
            var collider = target.Get<ColliderComponent>();
            hitPnt = target.GetPosition();
            if (collider.Collider != null) {
                hitPnt += new Vector3(0, collider.Collider.bounds.size.y * 0.5f, 0);
            }
            dir = (hitPnt - origin.GetPosition()).normalized;
            if (target == origin) {
                hitPnt += dir * 1;
            }
            else if (collider.Collider != null) {
                //dir = hitPnt - Owner.Tr.position;
                //dir = dir.normalized;
                //hitPnt += (Owner.WorldCenter - hitPnt).normalized * targetActor.Collider.bounds.size.z * 0.5f;
                hitPnt += dir * collider.Collider.bounds.size.z * 0.5f;
            }
        }

        public static float HitMultiplier(int hit, GenericStats stats) {
            var multi = 1f;
            if (hit == CollisionResult.CriticalHit) {
                var critStat = stats.Get(Stats.CriticalMulti);
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

        public static CollisionEvent BuildEvent(Entity owner, Entity target) {
            GenerateHitLocDir(owner, target, out var hitPnt, out var dir);
            return new CollisionEvent(owner, target, hitPnt, dir);
        }
    }
}
