using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SensorDetectingNode : BaseNode {

        public CachedComponent<GridPosition> Position = new CachedComponent<GridPosition>();
        public CachedComponent<SensorComponent> Sensor = new CachedComponent<SensorComponent>();

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() { Position, Sensor };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(GridPosition), typeof(SensorComponent)
            };
        }
    }
}
