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

        public void Handle(DeathEvent arg) {
            var spawnOnDeath = arg.Target.Entity.Get<SpawnPrefabOnDeath>();
            if (spawnOnDeath != null) {
                SpawnPrefab(spawnOnDeath, arg);
            }
        }

        public void Handle(ActionEvent arg) {
            var data = arg.Action.Entity.Find<ActionFxComponent>()?.Value;
            if (data != null) {
                data.TriggerEvent(arg);
            }
        }

        public void Handle(PerformedCollisionEvent arg) {
            var data = arg.Origin.Entity.Find<ActionFxComponent>()?.Value;
            if (data != null) {
                TriggerCollisionEvent(data, arg.HitPoint, arg.HitNormal, arg.Target?.Entity);
            }
        }

        public void Handle(EnvironmentCollisionEvent arg) {
            var data = arg.EntityHit.Find<ActionFxComponent>()?.Value;
            if (data != null) {
                TriggerCollisionEvent(data, arg.HitPoint, arg.HitNormal, arg.EntityHit);
            }
        }

        public void Handle(CollisionEvent arg) {
            var data = arg.Source.Find<ActionFxComponent>()?.Value;
            if (data != null) {
                TriggerCollisionEvent(data, arg.HitPoint, arg.HitNormal, arg.Target?.Entity);
            }
        }

        private void TriggerCollisionEvent(ActionFx data, Vector3 hitPoint, Vector3 hitNormal, Entity target) {
            data.TriggerEvent(
                ActionState.Collision, hitPoint + (hitNormal * 0.1f), Quaternion.LookRotation(hitNormal),
                target?.GetTemplate<CharacterTemplate>());
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
                ItemPool.Spawn(spawnComponent.Prefab, spawn => {
                    spawn.Transform.position = Vector3.Lerp(spawnPos, spawnPos + Vector3.up, Random.value);
                    var rb = spawn.GetComponent<FakePhysicsObject>();
                    if (rb == null) {
                        return;
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
                });
            }
        }
    }
}
