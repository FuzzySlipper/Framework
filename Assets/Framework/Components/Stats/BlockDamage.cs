using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    
    [System.Serializable]
	public sealed class BlockDamage : IComponent {

        public List<Func<DamageEvent, bool>> Dels = new List<Func<DamageEvent, bool>>();

        public void Handle(ref DamageEvent arg) {
            if (arg.Amount <= 0) {
                return;
            }
            for (int i = 0; i < Dels.Count; i++) {
                if (Dels[i](arg)) {
                    arg.Amount = 0;
                    break;
                }
            }
        }
        
        public BlockDamage(){}

        public BlockDamage(SerializationInfo info, StreamingContext context) {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
        }

    }
}
