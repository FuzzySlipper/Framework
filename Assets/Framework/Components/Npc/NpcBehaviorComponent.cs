using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class NpcBehaviorComponent : IComponent {
        

        private Timer _timer = new Timer(4f, false);
        private Timer _wanderTimer = new Timer(6f, false);

        public Timer Timer { get => _timer; }
        public Timer WanderTimer { get => _wanderTimer; }
        public States State = States.Neutral;
        
        public NpcBehaviorComponent(){}
        
        public NpcBehaviorComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }

        public enum States {
            Neutral,
            Chasing,
            Attacking,
            Dead,
            Stunned,
            Waiting,
        }
    }
}
