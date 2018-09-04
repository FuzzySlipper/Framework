using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class HealImpact : IActionImpact {

        private int _targetVital;
        private float _damagePercent;
        private ActionFx _actionFx;

        public HealImpact(int targetVital, float damagePercent, ActionFx actionFx) {
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
            var stats = owner.Get<GenericStats>();
            if (stats == null) {
                return;
            }
            new HealEvent(stats.GetValue(Stats.Power) * _damagePercent, owner, target, _targetVital).Post(target);
        }
    }
}
