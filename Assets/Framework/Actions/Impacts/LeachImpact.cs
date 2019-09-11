using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [KnownType(typeof(IActionImpact))]
    [Priority(Priority.Lowest)]
    public class LeachImpact : IActionImpact {

        private string _damageType;
        private string _targetVital;
        private float _damagePercent;
        private CachedStat<BaseStat> _stat;

        public float Power { get { return _stat.Value * _damagePercent; } }

        public LeachImpact(Entity entity, string damageType, string targetVital, float damagePercent, BaseStat stat) {
            _damageType = damageType;
            _targetVital = targetVital;
            _damagePercent = damagePercent;
            _stat = new CachedStat<BaseStat>(entity, stat);
        }

        public void ProcessImpact(CollisionEvent collisionEvent, ActionStateEvent stateEvent) {
            if (collisionEvent.Hit <= 0) {
                return;
            }
            var amt = collisionEvent.Origin.Stats.GetValue(Stats.Power) * _damagePercent * CollisionExtensions.GetHitMultiplier(collisionEvent.Hit, collisionEvent.Origin);
            collisionEvent.Target.Post(new DamageEvent(amt, collisionEvent.Origin, collisionEvent.Target, _damageType, _targetVital));
            collisionEvent.Origin.Post(new HealEvent(amt, collisionEvent.Target, collisionEvent.Origin, _targetVital));
        }

        public LeachImpact(SerializationInfo info, StreamingContext context) {
            _damageType = info.GetValue(nameof(_damageType), _damageType);
            _targetVital = info.GetValue(nameof(_targetVital), _targetVital);
            _damagePercent = info.GetValue(nameof(_damagePercent), _damagePercent);
            _stat = info.GetValue(nameof(_stat), _stat);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_damageType), _damageType);
            info.AddValue(nameof(_targetVital), _targetVital);
            info.AddValue(nameof(_damagePercent), _damagePercent);
            info.AddValue(nameof(_stat), _stat);
        }
    }
}
