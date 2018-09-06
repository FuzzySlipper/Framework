using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DamageImpact : IActionImpact {

        private int _damageType;
        private int _targetVital;
        private float _normalizedPercent;
        private ActionFx _actionFx;

        public DamageImpact(int damageType, int targetVital, float normalizedPercent, ActionFx actionFx) {
            _damageType = damageType;
            _targetVital = targetVital;
            _normalizedPercent = normalizedPercent;
            _actionFx = actionFx;
        }

        public void ProcessAction(CollisionEvent collisionEvent, ActionStateEvent stateEvent, Entity owner, Entity target) {
            if (collisionEvent.Hit <= 0) {
                return;
            }
            if (_actionFx != null) {
                _actionFx.TriggerEvent(stateEvent);
            }
            var stats = owner.Find<GenericStats>();
            if (stats == null) {
                return;
            }
            new DamageEvent(stats.GetValue(Stats.Power) * _normalizedPercent * CollisionExtensions.HitMultiplier(collisionEvent.Hit, stats), owner, target, _damageType, _targetVital).Post(target);
        }
    }
}
