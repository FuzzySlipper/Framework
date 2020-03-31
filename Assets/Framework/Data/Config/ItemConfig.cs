using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace PixelComrades {
    public class ItemConfig : ScriptableObject {

        public string Name;
        public string ItemType;
        public AssetReferenceSprite Icon;
        public string ModifierGroup;
        public string Description;
        public ItemRarity Rarity;
        public int Price;
        public List<string> GenericComponents = new List<string>();
        public string ID { get { return name; } }

        public virtual void AddComponents(Entity entity) {
            entity.Add(new InventoryItem(1, Price, Rarity));
            //var stats = entity.Get<StatsContainer>();
            //stats.GetOrAdd(Stats.Weight).ChangeBase(data.GetValue<int>(DatabaseFields.Weight));
        }
    }
}