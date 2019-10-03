using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class AnimationNodeTrigger {

        private const float MinTriggerTime = 0.1f;
        
        public string Key { get; }
        public bool Triggered { get; private set; }

        private float _timeTriggered;
        private float _minTriggerTime;
        
        public AnimationNodeTrigger(string key, float minTriggerTime = MinTriggerTime) {
            Key = key;
            _minTriggerTime = minTriggerTime;
        }

        public void Reset() {
            Triggered = false;
        }

        public void Trigger() {
            if (Triggered) {
                return;
            }
            var currentTime = TimeManager.TimeUnscaled; 
            if (currentTime - _timeTriggered < _minTriggerTime) {
                return;
            }
            Triggered = true;
            _timeTriggered = currentTime;
        }
    }
}
