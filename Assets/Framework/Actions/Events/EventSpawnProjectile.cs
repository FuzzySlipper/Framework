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
            ProjectileFactory.SpawnProjectile(ae.Action.Entity, Data, ae.Position, spawnPos, spawnRot, ae.Action.Fx?.Value);
        }
    }
}
