using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Lowest)]
    public class LeachImpact : IActionImpact, IReceive<DamageEvent> {

        private int _damageType;
        private int _targetVital;
        private float _damagePercent;
        private ActionFx _actionFx;

        public LeachImpact(int damageType, int targetVital, float damagePercent, ActionFx actionFx) {
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
            var stats = owner.Get<GenericStats>();
            if (stats == null) {
                return;
            }
            new DamageEvent(stats.GetValue(Stats.Power) * _damagePercent * CollisionExtensions.HitMultiplier(collisionEvent.Hit, stats), owner, target, _damageType, _targetVital).Post(target);
        }

        public void Handle(DamageEvent arg) {
            arg.Target.RemoveObserver(this);
            new HealEvent(arg.Amount, arg.Target, arg.Origin, arg.TargetVital).Post(arg.Origin);
        }
    }
}
