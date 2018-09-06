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
            var spawnComponent = entity.Get<ActionSpawnComponent>();
            if (spawnComponent.Prefab == null) {
                Owner.DefaultPostAdvance(this);
                return;
            }
            var spawnTr = entity.Find<AnimTr>().Tr;
            Vector3 spawnPos;
            Quaternion spawnRot;
            if (spawnTr != null) {
                spawnPos = spawnTr.position;
                spawnRot = spawnTr.rotation;
            }
            else if (Owner.Target != null) {
                spawnPos = Owner.Target.GetPosition;
                spawnRot = Owner.Target.GetRotation;
            }
            else if (Owner.LastStateEvent != null) {
                spawnPos = Owner.LastStateEvent.Value.Position;
                spawnRot = Owner.LastStateEvent.Value.Rotation;
            }
            else {
                spawnPos = entity.GetPosition();
                spawnRot = entity.GetRotation();
            }
            var spawn = ItemPool.Spawn(spawnComponent.Prefab, spawnPos, spawnRot);
            if (spawn == null) {
                Owner.DefaultPostAdvance(this);
                return;
            }
            var spawnEntity = Entity.New();
            spawnEntity.Add(new TransformComponent(spawn.Transform));
            MonoBehaviourToEntity.RegisterToEntity(spawn.gameObject, spawnEntity);
            Vector3 target;
            if (Owner.Target != null) {
                target = Owner.CurrentData == CollisionResult.Miss ? CollisionExtensions.MissPosition(Owner.Target) : Owner.Target.GetPosition;
            }
            else {
                var range = 15f;
                var stats = entity.Get<GenericStats>();
                if (stats != null) {
                    if (!stats.GetValue(Stats.Range, out range)) {
                        range = 15f;
                    }
                }
                target = spawnPos + spawnRot.eulerAngles * range;
            }
            spawnEntity.ParentId = entity.Id;
            spawnEntity.Add(new MoveSpeed(spawnEntity, spawnComponent.Speed));
            spawnEntity.Add(new RotationSpeed(spawnEntity, spawnComponent.Speed));
            spawnEntity.Add(new SimplerMover(spawnEntity));
            spawnEntity.AddObserver(this);
            spawnEntity.Post(new StartMoveEvent(spawnEntity, target, null));
        }

        public void Handle(MoveComplete arg) {
            var target = EntityController.GetEntity(arg.Target);
            Owner.PostAdvance(arg.Target, arg.MoveTarget, target.GetRotation(), StateEvent);
            target.Destroy();
        }
    }
}
