using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SensorToolkit;

namespace PixelComrades {
    [System.Serializable]
	public sealed class UnitySensorComponent : IComponent {
        private CachedUnityComponent<Sensor> _sensor;
        public Sensor Sensor { get { return _sensor.Value; } }

        public UnitySensorComponent(Sensor sensor) {
            _sensor = new CachedUnityComponent<Sensor>(sensor);
        }

        public UnitySensorComponent(SerializationInfo info, StreamingContext context) {
            _sensor = info.GetValue(nameof(_sensor), _sensor);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_sensor), _sensor);
        }
    }
}
