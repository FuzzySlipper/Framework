using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EventSpawnProjectile : IActionEventHandler {
        public ActionState State { get; }
        public string Data { get; }

        public EventSpawnProjectile(ActionState state, string data) {
            State = state;
            Data = data;
        }

        public void Trigger(ActionEvent ae, string eventName) {
            var target = ae.Position;
            Entity spawnEntity;
            var node = ae.Origin;
            var spawnPivot = ae.Origin.Get<SpawnPivotComponent>();
            if (spawnPivot != null) {
                spawnEntity = World.Get<ProjectileSystem>().SpawnProjectile(
                    node.CurrentAction.Entity, Data, target,
                    spawnPivot.position, spawnPivot.rotation);
            }
            else {
                var animData = node.Animator.Value;
                var spawnPos = animData?.GetEventPosition ?? (node.Tr != null ? node.Tr.position : Vector3.zero);
                var spawnRot = animData?.GetEventRotation ?? (node.Tr != null ? node.Tr.rotation : Quaternion.identity);
                DebugExtension.DebugPoint(spawnPos, Color.blue, 1f, 1f);
                spawnEntity = World.Get<ProjectileSystem>().SpawnProjectile(
                    node.CurrentAction.Entity, Data, ae.Position, 
                spawnPos, spawnRot);
            }
            if (spawnEntity != null) {
                if (node.CurrentAction.Fx != null) {
                    var afx = spawnEntity.Get<ActionFxComponent>();
                    if (afx != null) {
                        afx.ChangeFx(node.CurrentAction.Fx);
                    }
                    else {
                        spawnEntity.Add(new ActionFxComponent(node.CurrentAction.Fx));
                    }
                }
            }
        }
    }
}
