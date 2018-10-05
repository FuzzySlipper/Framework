using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpawnProjectile : ICommandElement {

        public CommandSequence Owner { get; set; }
        public ActionStateEvents StateEvent { get; }

        public SpawnProjectile(ActionStateEvents stateEvent) {
            StateEvent = stateEvent;
        }

        public void Start(Entity entity) {
            var spawnEntity = World.Get<ProjectileSystem>().SpawnProjectile(entity);
            if (spawnEntity == null) {
                Owner.DefaultPostAdvance(this);
                return;
            }
            var actionTarget = entity.Find<CommandTarget>();
            if (actionTarget?.TargetTr == null) {
                var rb = spawnEntity.Get<RigidbodyComponent>();
                if (rb != null && rb.Rb) {
                    rb.velocity = entity.Find<RigidbodyComponent>().velocity;
                    rb.AddForce(spawnEntity.Get<TransformComponent>().forward * spawnEntity.Get<MoveSpeed>());
                }
            }
            else {
                var mvTarget = actionTarget.TargetTr == null ? new MoveTarget(actionTarget.TargetTr.Tr.c.Tr) : new MoveTarget(actionTarget.GetPosition);
                spawnEntity.Add(mvTarget);
            }
            Owner.DefaultPostAdvance(this);
        }
    }
}
