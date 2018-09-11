using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpawnPhysicsProjectile : ICommandElement {

        public CommandSequence Owner { get; set; }
        public ActionStateEvents StateEvent { get; }
        public float TimeOut = 45;

        public SpawnPhysicsProjectile(ActionStateEvents stateEvent) {
            StateEvent = stateEvent;
        }

        public void Start(Entity entity) {
            var spawnComponent = entity.Get<ActionSpawnComponent>();
            if (spawnComponent.Prefab == null) {
                Owner.DefaultPostAdvance(this);
                return;
            }
            entity.FindSpawn(out var spawnPos, out var spawnRot);
            var spawn = ItemPool.Spawn(spawnComponent.Prefab, spawnPos, spawnRot);
            if (spawn == null) {
                Owner.DefaultPostAdvance(this);
                return;
            }
            var spawnEntity = Entity.New("Spawn");
            spawnEntity.Add(new TransformComponent(spawn.Transform));
            spawnEntity.Add(new RigidbodyComponent(spawn.GetComponent<Rigidbody>()));
            MonoBehaviourToEntity.RegisterToEntity(spawn.gameObject, spawnEntity);
            spawnEntity.ParentId = entity.Id;
            spawnEntity.Add(new MoveSpeed(spawnEntity, spawnComponent.Speed));
            spawnEntity.Add(new RotationSpeed(spawnEntity, spawnComponent.Rotation));
            spawnEntity.Add(new VelocityMover());
            spawnEntity.Add(new CollisionCheckForward(10));
            var actionTarget = entity.Find<CommandTarget>();
            if (actionTarget?.TargetTr == null) {
                spawnEntity.Get<RigidbodyComponent>().Rb.AddForce(spawn.transform.forward * spawnComponent.Speed);
            }
            else {
                var mvTarget = actionTarget.TargetTr == null ? new MoveTarget(actionTarget.TargetTr.Tr.c.Tr) : new MoveTarget(actionTarget.GetPosition);
                spawnEntity.Add(mvTarget);
            }
        }
    }
}
