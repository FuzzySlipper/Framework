using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace PixelComrades {
    public class ItemTemplate : ScriptableObject, IGenericData {

        [SerializeField] private string _id = "";
        [SerializeField] private string _name = "";
        [SerializeField] private Sprite _icon = null;
        [SerializeField] private int _price = 0;
        [SerializeField] private int _maxStack = 0;
        [SerializeField] private int _rarity = 0;
        [SerializeField] private string _description = "";
        [SerializeField] private int _weight;
        [SerializeField] private ModifierGroups _modifierGroup = ModifierGroups.None;
        [SerializeField] private int _itemType = 0;

        public int ItemType { get { return _itemType; } protected set { _itemType = value; } }
        public ModifierGroups ModifierGroup { get { return _modifierGroup; } protected set { _modifierGroup = value; } }
        public int Weight { get { return _weight; } protected set { _weight = value; } }
        public string Description { get { return _description; } protected set { _description = value; } }
        public int MaxStack { get { return _maxStack; } protected set { _maxStack = value; } }
        public int Rarity { get { return _rarity; } protected set { _rarity = value; } }
        public int Price { get { return _price; } protected set { _price = value; } }
        public string Id { get { return _id; } protected set { _id = value; } }
        public string Name { get { return _name; } protected set { _name = value; } }
        public Sprite Icon { get { return _icon; } protected set { _icon = value; } }

        public Entity New() {
            return New(1, null, null);
        }

        public virtual Entity New(int level, ItemModifier prefix, ItemModifier suffix) {
            var entity = Entity.New(Name);
            SetupBaseItem(entity, level);
            return entity;
        }

        protected void SetupBaseItem(Entity entity, int level) {
            entity.Add(new TypeId(Id));
            entity.Add(new InventoryItem(MaxStack, Price, Rarity));
            entity.Add(new LabelComponent(Name));
            entity.Add(new DescriptionComponent(Description));
            entity.Add(new IconComponent(Icon));
            entity.Add(new EntityLevelComponent(level));
        }


        public void OverrideIcon(Sprite icon) {
            _icon = icon;
        }

        public virtual void ConfigFromStrings(IList<string> lines) {
            
        }
    }
}
