using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class FloatingMessages : IComponent, IReceive<DamageEvent>, IReceive<HealEvent> {
        
        public FloatingMessages() {}
        public void GetObjectData(SerializationInfo info, StreamingContext context) {}
        public FloatingMessages(SerializationInfo info, StreamingContext context) {}

        public void Handle(DamageEvent arg) {
            UIFloatingText.WorldSpawn(arg.Amount.ToString("F0"), this.GetEntity().GetPosition(), Color.red);
        }

        public void Handle(HealEvent arg) {
            UIFloatingText.WorldSpawn(arg.Amount.ToString("F0"), this.GetEntity().GetPosition(), Color.green);
        }
    }
}
