using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SensorToolkit;

namespace PixelComrades {
    public class UnitySensorComponent : ComponentBase {
        public Sensor Sensor { get; }

        public UnitySensorComponent(Sensor sensor) {
            Sensor = sensor;
        }
    }
}
