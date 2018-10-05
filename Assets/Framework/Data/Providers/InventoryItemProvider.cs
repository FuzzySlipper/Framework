using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class InventoryItemProvider : IDataFactory<InventoryItem> {

        public void AddComponent(Entity entity, DataEntry data) {
            entity.Add(new InventoryItem(data.TryGetValue(DatabaseFields.MaxStack, 1), data.TryGetValue(DatabaseFields.Price, 1), data.TryGetValue(DatabaseFields.Rarity, 0)));
            entity.Stats.GetOrAdd(Stats.Weight).ChangeBase(data.GetValue<int>(DatabaseFields.Weight));
        }
    }
}
