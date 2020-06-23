using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Serializable]
    public class CostItself : CommandCost, ISerializable {

        public override void ProcessCost(ActionTemplate action, CharacterTemplate owner) {
            action.Get<InventoryItem>()?.Inventory?.Remove(owner);
            action.Entity.Destroy();
        }

        public CostItself() {
        }

        public CostItself(SerializationInfo info, StreamingContext context) {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
        }

        public override bool CanAct(ActionTemplate action, CharacterTemplate owner) {
            return true;
        }
    }
}
