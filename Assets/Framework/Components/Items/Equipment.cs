using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    public class Equipment : IComponent, IReceive<DataDescriptionAdded> {
        private int _owner = -1;
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
        
        private EquipmentSlot _equip;
        private StatModHolder[] _mods;
        private List<string> _statsToEquip = new List<string>();

        public bool Equipped { get { return _equip != null; } }
        public EquipmentSlot EquipmentSlot { get { return _equip; } }
        public string EquipmentSlotType { get; set; }

        public Equipment(string equip) {
            EquipmentSlotType = equip;
        }

        public void Handle(Entity entity) {
            entity.GetOrAdd<TooltipComponent>().OnTooltipDel += OnTooltip;
        }

        public void AddStat(string stat) {
            if (_statsToEquip.Contains(stat)) {
                return;
            }
            _statsToEquip.Add(stat);
        }
        
        public void OnTooltip(IComponent component) {
            var container = this.Get<InventoryItem>();
            if (container?.Inventory == null) {
                return;
            }
            var compareItem = container.Inventory.GetEntity().Get<EquipmentSlots>()?.GetSlot(EquipmentSlotType).Item ?? null;
            if (compareItem != null && compareItem.Id != Owner) {
                Game.DisplayCompareData(compareItem);
            }
        }

        private void ClearCurrentMods() {
            if (_mods == null) {
                return;
            }
            for (int i = 0; i < _mods.Length; i++) {
                _mods[i].Remove();
            }
        }

        public void Equip(EquipmentSlot slot) {
            ClearCurrentMods();
            if (_mods == null || _mods.Length != _statsToEquip.Count) {
                var owner = this.GetEntity();
                _mods = new StatModHolder[_statsToEquip.Count];
                for (int i = 0; i < _mods.Length; i++) {
                    _mods[i] = new DerivedStatModHolder(owner.Stats.Get(_statsToEquip[i]), 1);
                }
            }
            _equip = slot;
            if (slot == null) {
                return;
            }
            for (int i = 0; i < _mods.Length; i++) {
                _mods[i].Attach(_equip.SlotOwner.GetEntity().Stats.Get(_mods[i].StatID));
            }
        }

        public void UnEquip() {
            ClearCurrentMods();
            _equip = null;
        }

        public bool TryRemoveFromSlot() {
            if (_equip == null) {
                return false;
            }
            _equip.RemoveItemAddToOwnInventory();
            return true;
        }

        public void Handle(DataDescriptionAdded arg) {
            var entity = this.GetEntity();
            FastString.Instance.Clear();
            for (int i = 0; i < _statsToEquip.Count; i++) {
                FastString.Instance.AppendNewLine(entity.Stats.Get(_statsToEquip[i]).ToLabelString());
            }
            FastString.Instance.AppendBoldLabelNewLine("Equipment Slot", GameData.EquipmentSlotTypes.GetNameAt(EquipmentSlotType));
            arg.Data.Text += FastString.Instance.ToString();
        }
    }
}
