using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class StunImpact : IActionImpact {

        private float _chance;
        private float _length;

        public float Power { get { return 0; } }

        public StunImpact(float chance, float length) {
            _chance = chance;
            _length = length;
        }

        public void ProcessImpact(CollisionEvent collisionEvent, ActionStateEvent stateEvent) {
            if (collisionEvent.Hit <= 0 || !Game.DiceRollSuccess(_chance)) {
                return;
            }
            collisionEvent.Target.Post(new StunEvent(collisionEvent.Target, _length, true));
        }

        public StunImpact(SerializationInfo info, StreamingContext context) {
            _length = info.GetValue(nameof(_length), _length);
            _chance = info.GetValue(nameof(_chance), _chance);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_length), _length);
            info.AddValue(nameof(_chance), _chance);
        }
    }
}
