using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class WaitForSpawnMovement : ICommandElement, IReceive<MoveComplete> {

        public CommandSequence Owner { get; set; }
        public ActionStateEvents StateEvent { get; }

        public WaitForSpawnMovement(ActionStateEvents stateEvent) {
            StateEvent = stateEvent;
        }

        public void Start(Entity entity) {
            var spawnEntity = World.Get<ProjectileSystem>().SpawnProjectile(entity);
            if (spawnEntity == null) {
                Owner.DefaultPostAdvance(this);
                return;
            }
            var tr = spawnEntity.Tr;
            Vector3 target;
            if (Owner.Target != null) {
                target = Owner.CurrentData == CollisionResult.Miss ? CollisionExtensions.MissPosition(Owner.Target) : Owner.Target.GetPosition;
            }
            else {
                if (!entity.Stats.GetValue(Stats.Range, out var range)) {
                    range = 15f;
                }
                target = tr.position + tr.rotation.eulerAngles * range;
            }
            spawnEntity.ParentId = entity.Id;
            spawnEntity.Add(new SimplerMover(spawnEntity));
            spawnEntity.AddObserver(this);
            spawnEntity.Post(new StartMoveEvent(spawnEntity, target, null));
        }

        public void Handle(MoveComplete arg) {
            var target = EntityController.GetEntity(arg.Target);
            Owner.PostAdvance(target, arg.MoveTarget, target.GetRotation(), StateEvent);
            target.Destroy();
        }
    }
}
