using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Serializable]
    public class CostItself : CommandCost, ISerializable {

        public override void ProcessCost(Entity owner, Entity action) {
            owner.Get<InventoryItem>()?.Inventory?.Remove(owner);
            owner.Destroy();
        }

        public CostItself() {
        }

        public CostItself(SerializationInfo info, StreamingContext context) {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
        }

        public override bool CanAct(Entity owner, Entity action) {
            return true;
        }
    }
}
