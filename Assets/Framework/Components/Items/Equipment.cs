using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PixelComrades {
    [System.Serializable]
	public sealed class Equipment : IComponent, IReceive<DataDescriptionAdded> {
        
        public Equipment(SerializationInfo info, StreamingContext context) {
            EquipmentSlotType = info.GetValue(nameof(EquipmentSlotType), EquipmentSlotType);
            _statsToEquip = info.GetValue(nameof(_statsToEquip), _statsToEquip);
            var equipEntity = info.GetValue("EquipEntity", -1);
            if (equipEntity < 0) {
                return;
            }
            var equipSlot = info.GetValue("EquipName", "");
            TimeManager.PauseFor(0.1f, true, () => EquipToSerializedEntity(equipEntity, equipSlot));
        }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(EquipmentSlotType), EquipmentSlotType);
            info.AddValue(nameof(_statsToEquip), _statsToEquip);
            info.AddValue("EquipEntity", _equip?.SlotOwner.GetEntity().Id ?? -1);
            info.AddValue("EquipName", _equip?.Name ?? "");
        }
        
        private List<string> _statsToEquip = new List<string>();
        private EquipmentSlot _equip;
        private StatModHolder[] _mods;

        public bool Equipped { get { return _equip != null; } }
        public EquipmentSlot EquipmentSlot { get { return _equip; } }
        public string EquipmentSlotType { get; }

        public Equipment(string equip) {
            EquipmentSlotType = equip;
        }

        private void EquipToSerializedEntity(int entityId, string equipName) {
            var targetEntity = EntityController.Get(entityId);
            var ourEntity = this.GetEntity();
            if (targetEntity == null || ourEntity == null) {
                return;
            }
            var slot = targetEntity.Get<EquipmentSlots>()?.GetSlotNameExact(equipName) ?? null;
            if (slot == null) {
                return;
            }
            slot.EquipItem(ourEntity);
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
            var equipSlots = container.Inventory.GetEntity().Get<EquipmentSlots>();
            if (equipSlots == null) {
                return;
            }
            var slot = equipSlots.GetSlot(EquipmentSlotType);
            if (slot == null) {
                return;
            }
            var compareItem = slot.Item ?? null;
            if (compareItem != null && compareItem.Id != this.GetEntity()) {
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
