using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ItemInventory : EntityContainer {

        public override bool CanAdd(Entity entity) {
            if (!base.CanAdd(entity)) {
                return false;
            }
            return entity.HasComponent<InventoryItem>();
        }

        public override bool Add(Entity item) {
            if (base.Add(item)) {
                CheckIndices();
                return true;
            }
            return false;
        }

        private void CheckIndices() {
            for (int i = 0; i < Count; i++) {
                var index = this[i].Get<InventoryItem>()?.Index ?? -1;
                if (index < 0) {
                    continue;
                }
                for (int c = 0; c < Count; c++) {
                    if (c == i) {
                        continue;
                    }
                    var otherIndex = this[c].Get<InventoryItem>()?.Index ?? -1;
                    if (otherIndex == index) {
                        this[c].Get<InventoryItem>().Index = -1;
                    }
                }
            }
        }
    }
}
