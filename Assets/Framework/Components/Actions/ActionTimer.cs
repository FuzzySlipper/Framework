using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ActionTimer : IComponent, IReceive<ActionStateEvent> {
        public int Owner { get; set; }
        public float ElapsedTime { get; private set; }
        private float _startTime;

        public ActionTimer(int owner) {
            Owner = owner;
        }

        public void Handle(ActionStateEvent arg) {
            if (arg.State == ActionStateEvents.Start) {
                ElapsedTime = 0;
                _startTime = TimeManager.Time;
            }
            else if (arg.State == ActionStateEvents.Activate) {
                ElapsedTime = TimeManager.Time - _startTime;
            }
            
        }
    }
}
