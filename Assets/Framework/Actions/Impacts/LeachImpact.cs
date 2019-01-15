using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Lowest)]
    public class LeachImpact : IActionImpact, IReceive<DamageEvent> {

        private string _damageType;
        private string _targetVital;
        private float _damagePercent;
        private ActionFx _actionFx;

        public LeachImpact(string damageType, string targetVital, float damagePercent, ActionFx actionFx) {
            _damageType = damageType;
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
            target.AddObserver(this);
            target.Post(new DamageEvent(owner.Stats.GetValue(Stats.Power) * _damagePercent * CollisionExtensions.GetHitMultiplier(collisionEvent.Hit, owner), owner, target, _damageType, _targetVital));
        }

        public void Handle(DamageEvent arg) {
            arg.Target.RemoveObserver(this);
            arg.Origin.Post(new HealEvent(arg.Amount, arg.Target, arg.Origin, arg.TargetVital));
        }
    }
}
