using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class HealImpact : IActionImpact, ISerializable {

        private string _targetVital;
        private float _normalizedPercent;
        private CachedStat<BaseStat> _stat;
        private bool _healSelf;

        public float Power { get { return _stat.Value * _normalizedPercent; } }

        public HealImpact(string targetVital, float normalizedPercent, BaseStat stat, bool healSelf) {
            _targetVital = targetVital;
            _normalizedPercent = normalizedPercent;
            _healSelf = healSelf;
            _stat = new CachedStat<BaseStat>(stat);
        }

        public void ProcessImpact(CollisionEvent collisionEvent, ActionStateEvent stateEvent) {
            if (collisionEvent.Hit <= 0) {
                return;
            }
            var target = _healSelf ? collisionEvent.Origin : collisionEvent.Target;
            target.Post(new HealEvent(Power, collisionEvent.Origin, target, _targetVital));
        }

        public HealImpact(SerializationInfo info, StreamingContext context) {
            _healSelf = info.GetValue(nameof(_healSelf), _healSelf);
            _targetVital = info.GetValue(nameof(_targetVital), _targetVital);
            _normalizedPercent = info.GetValue(nameof(_normalizedPercent), _normalizedPercent);
            _stat = info.GetValue(nameof(_stat), _stat);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_healSelf), _healSelf);
            info.AddValue(nameof(_targetVital), _targetVital);
            info.AddValue(nameof(_normalizedPercent), _normalizedPercent);
            info.AddValue(nameof(_stat), _stat);
        }
    }
}
