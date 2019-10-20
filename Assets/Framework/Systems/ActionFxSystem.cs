using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public class ActionFxSystem : SystemBase, IReceive<DeathEvent>, IReceive<ActionEvent>,
        IReceive<EnvironmentCollisionEvent>, IReceive<PerformedCollisionEvent>, IReceive<CollisionEvent> {
        
        public ActionFxSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(ActionFxComponent), typeof(SpawnPrefabOnDeath),
            }));
        }

        public void TriggerSpawn(SpawnOnEvent spawnEvent, ActionEvent arg) {
            if (arg.State == spawnEvent.End && spawnEvent.ActiveGameObject != null) {
                ItemPool.Despawn(spawnEvent.ActiveGameObject);
            }
            else if (arg.State == spawnEvent.Start && spawnEvent.Prefab != null) {
                arg.GetSpawnPositionRotation(out var spawnPos, out var spawnRot);
                spawnEvent.ActiveGameObject = ItemPool.Spawn(spawnEvent.Prefab, spawnPos, spawnRot);
                if (spawnEvent.End == ActionState.None) {
                    spawnEvent.ActiveGameObject = null;
                }
            }
        }

        public void Handle(DeathEvent arg) {
            var spawnOnDeath = arg.Target.Entity.Get<SpawnPrefabOnDeath>();
            if (spawnOnDeath != null) {
                SpawnPrefab(spawnOnDeath, arg);
            }
        }

        public void Handle(ActionEvent arg) {
            var data = arg.Origin.Entity.Find<ActionFxComponent>()?.Value;
            if (data != null) {
                data.TriggerEvent(arg);
            }
        }

        public void Handle(PerformedCollisionEvent arg) {
            var data = arg.Origin.Entity.Find<ActionFxComponent>()?.Value;
            if (data != null) {
                data.TriggerEvent(
                    new ActionEvent(
                        arg.Origin, arg.Target, arg.HitPoint + (arg.HitNormal * 0.1f), Quaternion.LookRotation(arg.HitNormal),
                        ActionState.Collision));
            }
        }

        public void Handle(EnvironmentCollisionEvent arg) {
            var data = arg.EntityHit.Find<ActionFxComponent>()?.Value;
            if (data != null) {
                data.TriggerEvent(
                    new ActionEvent(
                        arg.EntityHit, null, arg.HitPoint + (arg.HitNormal * 0.1f), Quaternion.LookRotation(arg.HitNormal),
                        ActionState.Collision));
            }
        }

        public void Handle(CollisionEvent arg) {
            var data = arg.Target.Entity.Find<ActionFxComponent>()?.Value;
            if (data != null) {
                data.TriggerEvent(
                    new ActionEvent(
                        arg.Origin, arg.Target, arg.HitPoint + (arg.HitNormal * 0.1f), Quaternion.LookRotation(arg.HitNormal),
                        ActionState.Collision));
            }
        }

        private void SpawnPrefab(SpawnPrefabOnDeath spawnComponent, DeathEvent arg) {
            var position = arg.Target.Tr.position;
            var count = spawnComponent.CountRange.Get();
            if (count <= 0) {
                return;
            }
            for (int i = 0; i < count; i++) {
                var spawnPos = position + Random.insideUnitSphere * (spawnComponent.Radius * 0.5f);
                spawnPos.y = position.y;
                var spawn = ItemPool.Spawn(UnityDirs.Items, spawnComponent.Prefab, 
                    Vector3.Lerp(spawnPos, spawnPos + Vector3.up, Random.value), Quaternion.identity, true);
                if (spawn == null) {
                    continue;
                }
                var rb = spawn.GetComponent<FakePhysicsObject>();
                if (rb == null) {
                    continue;
                }
                WhileLoopLimiter.ResetInstance();
                while (WhileLoopLimiter.InstanceAdvance()) {
                    var throwPos = spawnPos + (Random.insideUnitSphere * spawnComponent.Radius);
                    throwPos.y = position.y;
                    if (!Physics.Linecast(spawn.transform.position, throwPos, LayerMasks.Environment)) {
                        if (Physics.Raycast(throwPos, Vector3.down, out var hit, 5f, LayerMasks.Floor)) {
                            throwPos = hit.point;
                        }
                        rb.Throw(throwPos);
                        break;
                    }
                }
            }
        }
    }
}
