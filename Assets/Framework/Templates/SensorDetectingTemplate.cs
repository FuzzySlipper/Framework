﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SensorDetectingTemplate : BaseTemplate {

        private CachedComponent<GridPosition> _position = new CachedComponent<GridPosition>();
        private CachedComponent<SensorComponent> _sensor = new CachedComponent<SensorComponent>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        
        public GridPosition Position { get => _position; }
        public SensorComponent Sensor { get => _sensor; }
        public TransformComponent Tr { get => _tr?.Value; }
        
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() { _position, _sensor, _tr };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(GridPosition), typeof(SensorComponent), typeof(TransformComponent)
            };
        }
    }

    public class UnitySensorTemplate : BaseTemplate {

        private CachedComponent<SensorTargetsComponent> _targets = new CachedComponent<SensorTargetsComponent>();
        private CachedComponent<UnitySensorComponent> _sensor = new CachedComponent<UnitySensorComponent>();
        private CachedComponent<FactionComponent> _faction = new CachedComponent<FactionComponent>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        public SensorTargetsComponent Targets { get => _targets.Value; }
        public UnitySensorComponent Sensor { get => _sensor.Value; }
        public FactionComponent Faction { get => _faction.Value; }
        public TransformComponent Tr { get => _tr?.Value; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _targets, _sensor, _faction, _tr
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(SensorTargetsComponent), typeof(UnitySensorComponent), typeof(FactionComponent), typeof(TransformComponent)
            };
        }
    }
}
