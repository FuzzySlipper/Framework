using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EventSpawnProjectile : IActionEventHandler {
        public string Data { get; }

        public EventSpawnProjectile(string data) {
            Data = data;
        }

        public void Trigger(ActionEvent ae, string eventName) {
            var spawnPos = ae.Origin.AnimationEvent.Position;
            var spawnRot = ae.Origin.AnimationEvent.Rotation;
            var spawnEntity = World.Get<ProjectileSystem>().SpawnProjectile(
                ae.Action.Entity, Data, ae.Position,
                spawnPos, spawnRot);
            if (spawnEntity != null) {
                if (ae.Action.Fx != null) {
                    var afx = spawnEntity.Get<ActionFxComponent>();
                    if (afx != null) {
                        afx.ChangeFx(ae.Action.Fx.Value);
                    }
                    else {
                        spawnEntity.Add(new ActionFxComponent(ae.Action.Fx.Value));
                    }
                }
            }
        }
    }
}
