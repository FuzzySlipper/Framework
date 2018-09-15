using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    public class Equipment : IComponent{
        private int _owner;
        public int Owner {
            get { return _owner; }
            set {
                if (_owner == value) {
                    return;
                }
                _owner = value;
                if (_owner < 0) {
                    return;
                }
                Handle(this.GetEntity());
            }
        }
        
        private EquipmentTemplate _template = null;
        private bool _isMagic = false;
        private int _level;
        private EquipmentSlot _equip;

        public bool Equipped { get { return _equip != null; } }
        public EquipmentSlot EquipmentSlot { get { return _equip; } }
        public int Skill { get; set; }
        public int SkillRank { get; set; }
        public int EquipmentSlotType { get; set; }
        public EquipmentTemplate Template { get { return _template; } }
        public ItemEquipmentModifier PrefixEquip { get; }
        public ItemEquipmentModifier SuffixEquip { get; }

        public Equipment(EquipmentTemplate template, int level, ItemEquipmentModifier prefix, ItemEquipmentModifier suffix) {
            _template = template;
            PrefixEquip = prefix;
            SuffixEquip = suffix;
            _level = level;
        }

        public void SetMagic() {
            _isMagic = true;
        }

        public void Handle(Entity entity) {
            StringBuilder sb = new StringBuilder();
            if (PrefixEquip != null) {
                PrefixEquip.Init(_level, entity);
                sb.Append(PrefixEquip.DescriptiveName);
                sb.Append(" ");
            }
            sb.Append(_template.Name);
            if (SuffixEquip != null) {
                SuffixEquip.Init(_level, entity);
                sb.Append(" ");
                sb.Append(SuffixEquip.DescriptiveName);
            }
            entity.Add(new LabelComponent(sb.ToString()));
            entity.GetOrAdd<TooltipComponent>().OnTooltipDel += OnTooltip;
        }
        

        public void OnTooltip(IComponent component) {
            var container = this.Get<InventoryItem>();
            if (container?.Inventory == null) {
                return;
            }
            var compareItem = container.Inventory.GetEntity().Get<EquipmentSlots>()?.GetSlot(_template.EquipSlot).Item ?? null;
            if (compareItem != null && compareItem.Id != Owner) {
                Game.DisplayCompareData(compareItem);
            }
        }

        public void Equip(EquipmentSlot slot) {
            _equip = slot;
            if (PrefixEquip != null) {
                PrefixEquip.SetEquipped(this, true);
            }
            if (SuffixEquip != null) {
                SuffixEquip.SetEquipped(this, true);
            }
        }

        public void Unequip() {
            if (PrefixEquip != null) {
                PrefixEquip.SetEquipped(this, false);
            }
            if (SuffixEquip != null) {
                SuffixEquip.SetEquipped(this, false);
            }
            _equip = null;
        }

        public bool TryRemoveFromSlot() {
            if (_equip == null) {
                return false;
            }
            _equip.RemoveItemAddToOwnInventory();
            return true;
        }
    }
}
