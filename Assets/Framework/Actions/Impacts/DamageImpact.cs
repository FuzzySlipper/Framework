using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DamageImpact : IActionImpact {

        private string _damageType;
        private string _targetVital;
        private float _normalizedPercent;

        public DamageImpact(string damageType, string targetVital, float normalizedPercent) {
            _damageType = damageType;
            _targetVital = targetVital;
            _normalizedPercent = normalizedPercent;
        }

        public void ProcessAction(CollisionEvent collisionEvent, ActionStateEvent stateEvent, Entity owner, Entity target) {
            if (collisionEvent.Hit <= 0) {
                return;
            }
            var stat = owner.FindStat<BaseStat>(Stats.Power);
            if (stat == null) {
                return;
            }
            target.Post(new DamageEvent(stat.Value * _normalizedPercent * CollisionExtensions.GetHitMultiplier(collisionEvent.Hit, owner), owner, target, _damageType, _targetVital));
        }
    }
}
