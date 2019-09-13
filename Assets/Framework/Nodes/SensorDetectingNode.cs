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

    public class UnitySensorNode : BaseNode {

        private CachedComponent<SensorTargetsComponent> _targets = new CachedComponent<SensorTargetsComponent>();
        private CachedComponent<UnitySensorComponent> _sensor = new CachedComponent<UnitySensorComponent>();
        private CachedComponent<FactionComponent> _faction = new CachedComponent<FactionComponent>();

        public SensorTargetsComponent Targets { get => _targets.Value; }
        public UnitySensorComponent Sensor { get => _sensor.Value; }
        public FactionComponent Faction { get => _faction.Value; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {_targets, _sensor, _faction};

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(SensorTargetsComponent), typeof(UnitySensorComponent), typeof(FactionComponent)
            };
        }
    }
}
