using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public struct CooldownComponent : IComponent {
        public Timer Cooldown;

        public CooldownComponent(float length, bool unscaled) : this() {
            Cooldown = new Timer(length, unscaled);
        }

        public void ActivateTimer(float length) {
            if (Cooldown != null) {
                Cooldown.StartNewTime(length);
                this.GetEntity().Post(EntitySignals.CooldownTimerChanged);
            }
        }

        public CooldownComponent(SerializationInfo info, StreamingContext context) {
            Cooldown = (Timer) info.GetValue(nameof(Cooldown), typeof(Timer));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Cooldown), Cooldown);
        }
    }
}
