using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class HealImpact : IActionImpact {

        private string _targetVital;
        private float _damagePercent;
        private ActionFx _actionFx;

        public HealImpact(string targetVital, float damagePercent, ActionFx actionFx) {
            _targetVital = targetVital;
            _damagePercent = damagePercent;
            _actionFx = actionFx;
        }

        public void ProcessAction(CollisionEvent collisionEvent, ActionStateEvent stateEvent, Entity owner, Entity target) {
            if (collisionEvent.Hit <= 0) {
                return;
            }
            if (_actionFx != null) {
                _actionFx.TriggerEvent(stateEvent);
            }
            target.Post(new HealEvent(collisionEvent.Target.Stats.GetValue(Stats.Power) * _damagePercent, owner, target, _targetVital));
        }
    }
}
