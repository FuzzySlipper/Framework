using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DamageImpact : IActionImpact {

        private string _damageType;
        private string _targetVital;
        private float _normalizedPercent;

        public string DamageType { get { return _damageType; } }

        public DamageImpact(string damageType, string targetVital, float normalizedPercent) {
            _damageType = damageType;
            _targetVital = targetVital;
            _normalizedPercent = normalizedPercent;
        }

        public void ProcessAction(CollisionEvent collisionEvent, ActionStateEvent stateEvent, Entity owner, Entity target) {
            if (collisionEvent.Hit <= 0) {
                return;
            }
            var stat = owner.FindStatValue(Stats.Power);
            target.Post(new DamageEvent(stat * _normalizedPercent * CollisionExtensions.GetHitMultiplier(collisionEvent.Hit, owner), owner, target, _damageType, _targetVital));
        }
    }
}
