using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class PhysicsSystem : SystemBase, IReceive<CollisionEvent> {
        private static GameOptions.CachedFloat _damageToPhysics = new GameOptions.CachedFloat("DamageToPhysicsRatio");
        private static GameOptions.CachedFloat _maxPhysicsDamage = new GameOptions.CachedFloat("MaxPhysicsDamage");
        
        public PhysicsSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(PhysicsOnDamageComponent)
            }));
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
            var power = arg.Source.Get<StatsContainer>()?.GetValue(Stats.Power) ?? 1;
            power *= _damageToPhysics;
            arg.Target.Post(new PhysicsInputMessage(arg.Target.Entity, dir * Mathf.Clamp(power, 1, _maxPhysicsDamage)));
        }   
    }
}
