using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class ConvertVitalImpact : IActionImpact {

        private float _percent;
        private string _sourceVital;
        private string _targetVital;

        public float Power { get { return _percent; } }

        public ConvertVitalImpact(float percent, string source, string target) {
            _percent = percent;
            _sourceVital = source;
            _targetVital = target;
        }

        public void ProcessImpact(CollisionEvent collisionEvent, ActionStateEvent stateEvent) {
            var ownerStats = stateEvent.Origin.Stats.GetVital(_sourceVital);
            var targetStats = stateEvent.Target.Stats.GetVital(_targetVital);
            if (ownerStats == null || targetStats == null) {
                return;
            }
            var amt = (targetStats.Current) * _percent;
            targetStats.Current -= amt;
            ownerStats.Current += amt;
        }

        public ConvertVitalImpact(SerializationInfo info, StreamingContext context) {
            _percent = info.GetValue(nameof(_percent), _percent);
            _sourceVital = info.GetValue(nameof(_sourceVital), _sourceVital);
            _targetVital = info.GetValue(nameof(_targetVital), _targetVital);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_percent), _percent);
            info.AddValue(nameof(_sourceVital), _sourceVital);
            info.AddValue(nameof(_targetVital), _targetVital);
        }
    }
}
