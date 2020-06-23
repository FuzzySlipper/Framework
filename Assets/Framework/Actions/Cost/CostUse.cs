using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Serializable]
    public class CostUse : CommandCost, ISerializable {

        public override void ProcessCost(Entity owner, Entity action) {
            owner.Get<LimitedUses>()?.Use();
        }

        public CostUse() {}

        public CostUse(SerializationInfo info, StreamingContext context) {}

        public void GetObjectData(SerializationInfo info, StreamingContext context) {}

        public override bool CanAct(Entity owner, Entity action) {
            var uses = owner.Get<LimitedUses>();
            if (uses == null || uses.Current == 0) {
                owner.PostAll(new StatusUpdate( owner,"No more uses", Color.yellow));
                return false;
            }
            return true;
        }
    }
}
