using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class ItemConfig : ScriptableObject, ICustomPreview {

        public string Name;
        public SpriteReference Icon;
        [TextArea]public string Description;
        public ItemRarity Rarity = ItemRarity.Common;
        public int Price;
        public string ID { get { return name; } }
        public virtual string ItemType { get { return ItemTypes.Item; } }
        public UnityEngine.Object Preview { get { return AssetReferenceUtilities.LoadAsset(Icon); }}
        public Object EditorObject { get { return this; } }

        public virtual void AddComponents(Entity entity) {
            entity.Add(new InventoryItem(1, Price, Rarity));
            //var stats = entity.Get<StatsContainer>();
            //stats.GetOrAdd(Stats.Weight).ChangeBase(data.GetValue<int>(DatabaseFields.Weight));
        }
    }
}