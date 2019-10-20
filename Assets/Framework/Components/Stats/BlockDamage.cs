using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    
    [System.Serializable]
	public sealed class BlockDamageFlat : IComponent {

        public BlockDamageFlat(){}

        public BlockDamageFlat(SerializationInfo info, StreamingContext context) {}

        public void GetObjectData(SerializationInfo info, StreamingContext context) {}
    }

    [System.Serializable]
    public sealed class BlockDamageWithStats : IComponent {

        public BlockDamageWithStats() {
        }

        public BlockDamageWithStats(SerializationInfo info, StreamingContext context) {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
        }
    }
}
