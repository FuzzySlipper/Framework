﻿using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {
    [System.Serializable]
    public class InstantKill : IActionImpact {

        private float _chance;

        public float Power { get { return _chance; } }

        public InstantKill(float chance) {
            _chance = chance;
        }

        public void ProcessImpact(CollisionEvent collisionEvent, ActionStateEvent stateEvent) {
            if (collisionEvent.Hit <= 0) {
                return;
            }
            if (Game.DiceRollSuccess(_chance)) {
                collisionEvent.Target.Post(new DeathEvent(stateEvent.Origin, stateEvent.Target, 100));
                collisionEvent.Target.Post(new CombatStatusUpdate(collisionEvent.Target,"Lethal Hit!", Color.red));
            }
        }

        public InstantKill(SerializationInfo info, StreamingContext context) {
            _chance = info.GetValue(nameof(_chance), _chance);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_chance), _chance);
        }
    }
}