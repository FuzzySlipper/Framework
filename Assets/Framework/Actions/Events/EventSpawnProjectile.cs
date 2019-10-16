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
            var node = ae.Origin;
            var spawnPos = node.AnimationEvent.LastEventPosition;
            var spawnRot = node.AnimationEvent.LastEventRotation;
            //ae.GetSpawnPositionRotation(out var spawnPos, out var spawnRot);
//            DebugExtension.DebugPoint(spawnPos, Color.blue, 1f, 1f);

            var spawnEntity = World.Get<ProjectileSystem>().SpawnProjectile(
                node.CurrentAction.Entity, Data, ae.Position,
                spawnPos, spawnRot);
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
