using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class HealImpact : IActionImpact {

        private string _targetVital;
        private float _damagePercent;

        public HealImpact(string targetVital, float damagePercent) {
            _targetVital = targetVital;
            _damagePercent = damagePercent;
        }

        public void ProcessAction(CollisionEvent collisionEvent, ActionStateEvent stateEvent, Entity owner, Entity target) {
            if (collisionEvent.Hit <= 0) {
                return;
            }
            target.Post(new HealEvent(collisionEvent.Target.FindStatValue(Stats.Power) * _damagePercent, owner, target, _targetVital));
        }
    }
}
