using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class DamageImpact : IActionImpact {

        private string _damageType;
        private string _targetVital;
        private float _normalizedPercent;
        private CachedStat<BaseStat> _stat;

        public float Power { get { return _stat.Value * _normalizedPercent; } }

        public DamageImpact(string damageType, string targetVital, float normalizedPercent, BaseStat stat) {
            _damageType = damageType;
            _targetVital = targetVital;
            _normalizedPercent = normalizedPercent;
            _stat = new CachedStat<BaseStat>(stat);
        }

        public void ProcessImpact(CollisionEvent collisionEvent, ActionStateEvent stateEvent) {
            if (collisionEvent.Hit <= 0) {
                return;
            }
            collisionEvent.Target.Post(new DamageEvent(Power * CollisionExtensions.GetHitMultiplier(collisionEvent.Hit, collisionEvent.Origin), collisionEvent.Origin, collisionEvent.Target, _damageType, _targetVital));
        }

        public DamageImpact(SerializationInfo info, StreamingContext context) {
            _damageType = info.GetValue(nameof(_damageType), _damageType);
            _targetVital = info.GetValue(nameof(_targetVital), _targetVital);
            _normalizedPercent = info.GetValue(nameof(_normalizedPercent), _normalizedPercent);
            _stat = info.GetValue(nameof(_stat), _stat);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_damageType), _damageType);
            info.AddValue(nameof(_targetVital), _targetVital);
            info.AddValue(nameof(_normalizedPercent), _normalizedPercent);
            info.AddValue(nameof(_stat), _stat);
        }
    }
}
