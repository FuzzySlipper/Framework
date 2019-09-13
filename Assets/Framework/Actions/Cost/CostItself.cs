using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Serializable]
    public class CostItself : CommandCost, ISerializable {

        public override void ProcessCost(Entity entity) {
            entity.Get<InventoryItem>(i => i.Inventory?.Remove(entity));
            entity.Destroy();
        }

        public CostItself() {
        }

        public CostItself(SerializationInfo info, StreamingContext context) {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
        }

        public override bool CanAct(Entity entity) {
            return true;
        }
    }
}
