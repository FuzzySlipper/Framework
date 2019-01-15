using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FloatingMessages : IComponent, IReceive<DamageEvent>, IReceive<HealEvent> {
        public int Owner { get; set; }

        public void Handle(DamageEvent arg) {
            UIFloatingText.WorldSpawn(arg.Amount.ToString("F0"), this.GetEntity().GetPosition(), Color.red);
        }

        public void Handle(HealEvent arg) {
            UIFloatingText.WorldSpawn(arg.Amount.ToString("F0"), this.GetEntity().GetPosition(), Color.green);
        }
    }
}
