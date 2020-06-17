using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class ItemConfig : ScriptableObject, ICustomPreview {

        [SerializeField] private string _name = "";
        [SerializeField, TextArea] private string _description = "";
        [SerializeField] private SpriteReference _icon = new SpriteReference();
        public ItemRarity Rarity = ItemRarity.Common;
        public int Price;
        public string Name { get => _name; }
        public string Description { get => _description; }
        public SpriteReference Icon { get => _icon; }
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