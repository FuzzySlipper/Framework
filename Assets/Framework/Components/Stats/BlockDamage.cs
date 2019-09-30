using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    
    [System.Serializable]
	public sealed class BlockDamage : IComponent, IDisposable {

        public List<Func<TakeDamageEvent, bool>> DamageBlockers = new List<Func<TakeDamageEvent, bool>>();
        public List<Func<CollisionEvent, int>> CollisionHandlers = new List<Func<CollisionEvent, int>>();
        public BlockDamage(){}

        public BlockDamage(SerializationInfo info, StreamingContext context) {}

        public void GetObjectData(SerializationInfo info, StreamingContext context) {}

        public void Dispose() {
            DamageBlockers = null;
            CollisionHandlers = null;
        }
    }
}
