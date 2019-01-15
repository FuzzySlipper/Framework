using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct CooldownComponent : IComponent {
        public Timer Cooldown;
        public int Owner { get; set; }

        public CooldownComponent(float length, bool unscaled) : this() {
            Cooldown = new Timer(length, unscaled);
        }

        public void ActivateTimer(float length) {
            if (Cooldown != null) {
                Cooldown.StartNewTime(length);
                this.GetEntity().Post(EntitySignals.CooldownTimerChanged);
            }
        }
    }
}
