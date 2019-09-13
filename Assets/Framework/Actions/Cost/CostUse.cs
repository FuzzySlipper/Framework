using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Serializable]
    public class CostUse : CommandCost, ISerializable {

        public override void ProcessCost(Entity entity) {
            entity.Get<LimitedUses>(f => f.Use());
        }

        public CostUse() {}

        public CostUse(SerializationInfo info, StreamingContext context) {}

        public void GetObjectData(SerializationInfo info, StreamingContext context) {}

        public override bool CanAct(Entity entity) {
            var uses = entity.Get<LimitedUses>();
            if (uses == null || uses.Current == 0) {
                entity.PostAll(new StatusUpdate("No more uses", Color.yellow));
                return false;
            }
            return true;
        }
    }
}
