using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public class ActionFxSystem : SystemBase, IReceive<DeathEvent>, IReceive<ActionStateEvent>,
        IReceive<EnvironmentCollisionEvent>, IReceive<PerformedCollisionEvent>, IReceive<CollisionEvent> {
        
        public ActionFxSystem() {
            EntityController.RegisterReceiver<SpawnPrefabOnDeath>(this);
            EntityController.RegisterReceiver<ActionFxComponent>(this);
        }

        public void TriggerSpawn(SpawnOnEvent spawnEvent, ActionStateEvent arg) {
            if (arg.State == spawnEvent.EndEvent && spawnEvent.ActiveGameObject != null) {
                ItemPool.Despawn(spawnEvent.ActiveGameObject);
            }
            else if (arg.State == spawnEvent.StartEvent && spawnEvent.Prefab != null) {
                var animData = arg.Origin.Entity.Find<AnimatorComponent>();
                var spawnPos = animData?.Value?.GetEventPosition ?? (arg.Origin.Tr != null ? arg.Origin.Tr.position : Vector3.zero);
                var spawnRot = animData?.Value?.GetEventRotation ??
                               (arg.Origin.Tr != null ? arg.Origin.Tr.rotation : Quaternion.identity);
                spawnEvent.ActiveGameObject = ItemPool.Spawn(spawnEvent.Prefab, spawnPos, spawnRot);
                if (spawnEvent.EndEvent == ActionStateEvents.None) {
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

        public void Handle(ActionStateEvent arg) {
            var data = arg.Origin.Entity.Find<ActionFxComponent>()?.Fx;
            if (data != null) {
                data.TriggerEvent(arg);
            }
        }

        public void Handle(PerformedCollisionEvent arg) {
            var data = arg.Origin.Entity.Find<ActionFxComponent>()?.Fx;
            if (data != null) {
                data.TriggerEvent(
                    new ActionStateEvent(
                        arg.Origin, arg.Target, arg.HitPoint + (arg.HitNormal * 0.1f), Quaternion.LookRotation(arg.HitNormal),
                        ActionStateEvents.Collision));
            }
        }

        public void Handle(EnvironmentCollisionEvent arg) {
            var data = arg.EntityHit.Find<ActionFxComponent>()?.Fx;
            if (data != null) {
                data.TriggerEvent(
                    new ActionStateEvent(
                        arg.EntityHit, null, arg.HitPoint + (arg.HitNormal * 0.1f), Quaternion.LookRotation(arg.HitNormal),
                        ActionStateEvents.Collision));
            }
        }

        public void Handle(CollisionEvent arg) {
            var data = arg.Target.Entity.Find<ActionFxComponent>()?.Fx;
            if (data != null) {
                data.TriggerEvent(
                    new ActionStateEvent(
                        arg.Origin, arg.Target, arg.HitPoint + (arg.HitNormal * 0.1f), Quaternion.LookRotation(arg.HitNormal),
                        ActionStateEvents.Collision));
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
                var spawn = ItemPool.Spawn(
                    UnityDirs.Items, spawnComponent.Prefab, Vector3.Lerp(spawnPos, spawnPos + Vector3.up, Random.value), Quaternion.identity, true);
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
