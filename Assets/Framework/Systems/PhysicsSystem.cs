using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class PhysicsSystem : SystemBase, IReceive<CollisionEvent> {
        private static GameOptions.CachedFloat _damageToPhysics = new GameOptions.CachedFloat("DamageToPhysicsRatio");
        private static GameOptions.CachedFloat _maxPhysicsDamage = new GameOptions.CachedFloat("MaxPhysicsDamage");
        
        public PhysicsSystem() {
            EntityController.RegisterReceiver<PhysicsOnDamageComponent>(this);
        }

        public void Handle(CollisionEvent arg) {
            if (!arg.Target.Entity.HasComponent<PhysicsOnDamageComponent>()) {
                return;
            }
            Vector3 dir;
            if (arg.HitNormal.sqrMagnitude > 0) {
                dir = -arg.HitNormal.normalized;
            }
            else {
                var originTr = arg.Origin.Tr;
                if (originTr == null) {
                    var parent = arg.Origin.Entity.GetParent();
                    if (parent != null) {
                        originTr = parent.Get<TransformComponent>();
                    }
                }
                var origin = originTr != null ? originTr.position : arg.HitPoint + (arg.HitNormal * 2);
                dir = (arg.HitPoint - origin).normalized;
            }
            var power = 1f;
            for (int i = 0; i < arg.Impacts.Count; i++) {
                power = Mathf.Max(arg.Impacts[i].Power * _damageToPhysics, power);
            }
            arg.Target.Post(new PhysicsInputMessage(arg.Target.Entity, dir * Mathf.Clamp(power, 1, _maxPhysicsDamage)));
        }   
    }
}
