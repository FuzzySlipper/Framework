using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class ActionTimer : IComponent, IReceive<ActionStateEvent> {
        public float ElapsedTime { get; private set; }
        private float _startTime;

        public ActionTimer() {}

        public void Handle(ActionStateEvent arg) {
            if (arg.State == ActionStateEvents.Start) {
                ElapsedTime = 0;
                _startTime = TimeManager.Time;
            }
            else if (arg.State == ActionStateEvents.Activate) {
                ElapsedTime = TimeManager.Time - _startTime;
            }
        }

        public ActionTimer(SerializationInfo info, StreamingContext context) {
            _startTime = info.GetValue(nameof(_startTime), _startTime);
            ElapsedTime = info.GetValue(nameof(ElapsedTime), ElapsedTime);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_startTime), _startTime);
            info.AddValue(nameof(ElapsedTime), ElapsedTime);
        }
    }
}
