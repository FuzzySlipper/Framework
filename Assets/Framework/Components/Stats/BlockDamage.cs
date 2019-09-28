using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    
    [System.Serializable]
	public sealed class BlockDamage : IComponent {

        public List<Func<TakeDamageEvent, bool>> Dels = new List<Func<TakeDamageEvent, bool>>();
        public BlockDamage(){}

        public BlockDamage(SerializationInfo info, StreamingContext context) {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
        }

    }
}
