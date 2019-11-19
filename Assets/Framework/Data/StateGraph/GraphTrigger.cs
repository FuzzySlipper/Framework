using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public sealed class GraphTrigger {
        public string Key;
        [SerializeField] private float _minTriggerTime = 0.1f;
        private float _timeTriggered;
        public bool Triggered { get; private set; }
        public float MinTriggerTime { get => _minTriggerTime; }
        public float TimeTriggered { get => _timeTriggered; }
        
        public void Reset() {
            Triggered = false;
        }

        public GraphTrigger() {}

        public GraphTrigger(GraphTrigger other) {
            Key = other.Key;
            _minTriggerTime = other._minTriggerTime;
        }

        public bool Trigger() {
            if (Triggered) {
                return false;
            }
            var currentTime = TimeManager.Time; 
            if (currentTime < _timeTriggered + _minTriggerTime) {
                return false;
            }
            Triggered = true;
            _timeTriggered = currentTime;
            return true;
        }
    }
    
}
