using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class EquipmentSystem : SystemBase, IReceiveGlobal<SaveGameLoaded>, IReceiveGlobal<DataDescriptionUpdating>,
    IReceiveGlobal<TooltipDisplaying>, IReceiveGlobal<EquipItemEvent>, IReceiveGlobal<UnEquipItemEvent> {
        
        public EquipmentSystem(){}
        
        public void HandleGlobal(SaveGameLoaded arg) {
            var equipArray = EntityController.GetComponentArray<Equipment>();
            foreach (Equipment equipment in equipArray) {
                if (equipment.Mods == null) {
                    continue;
                }
                for (int i = 0; i < equipment.Mods.Length; i++) {
                    equipment.Mods[i].Restore();
                }
            }
        }

        public void HandleGlobal(DataDescriptionUpdating arg) {
            var equipEntity = arg.Data.GetEntity();
            var equipment = equipEntity.Get<Equipment>();
            if (equipment == null) {
                return;
            }
            FastString.Instance.Clear();
            var stats = equipEntity.Get<StatsContainer>();
            for (int i = 0; i < equipment.StatsToEquip.Count; i++) {
                FastString.Instance.AppendNewLine(stats.Get(equipment.StatsToEquip[i]).ToLabelString());
            }
            FastString.Instance.AppendBoldLabelNewLine("Equipment Slot", GameData.EquipmentSlotTypes.GetNameAt(equipment.EquipmentSlotType));
            arg.Data.Text += FastString.Instance.ToString();
        }

        public void HandleGlobal(TooltipDisplaying arg) {
            var equipEntity = arg.Target.GetEntity();
            var equipment = equipEntity.Get<Equipment>();
            if (equipment == null) {
                return;
            }
            var container = equipEntity.Get<InventoryItem>();
            if (container?.Inventory == null) {
                return;
            }
            var equipSlots = container.Inventory.Owner.Get<EquipmentSlots>();
            if (equipSlots == null) {
                return;
            }
            var slot = equipSlots.GetSlot(equipment.EquipmentSlotType);
            if (slot == null) {
                return;
            }
            var compareItem = slot.Item ?? null;
            if (compareItem != null && compareItem.Id != arg.Target.GetEntity()) {
                Game.DisplayCompareData(compareItem);
            }
        }

        public void HandleGlobal(EquipItemEvent arg) {
            if (arg.Slot == null || arg.Equipment == null) {
                return;
            }
            arg.Equipment.ClearCurrentMods();
            var itemStats = arg.Equipment.GetEntity().Get<StatsContainer>();
            if (arg.Equipment.Mods == null || arg.Equipment.Mods.Length != arg.Equipment.StatsToEquip.Count) {
                arg.Equipment.Mods = new StatModHolder[arg.Equipment.StatsToEquip.Count];
                for (int i = 0; i < arg.Equipment.Mods.Length; i++) {
                    arg.Equipment.Mods[i] = new DerivedStatModHolder(itemStats.Get(arg.Equipment.StatsToEquip[i]), 1);
                }
            }
            var slotStats = arg.Slot.SlotOwner.GetEntity().Get<StatsContainer>();
            for (int i = 0; i < arg.Equipment.Mods.Length; i++) {
                arg.Equipment.Mods[i].Attach(slotStats.Get(arg.Equipment.Mods[i].StatID));
            }
        }

        public void HandleGlobal(UnEquipItemEvent arg) {
            arg.Equipment.ClearCurrentMods();
        }
    }

    public struct EquipItemEvent : IEntityMessage {
        public Equipment Equipment { get; }
        public EquipmentSlot Slot { get; }

        public EquipItemEvent(Equipment equipment, EquipmentSlot slot) {
            Equipment = equipment;
            Slot = slot;
        }
    }

    public struct UnEquipItemEvent : IEntityMessage {
        public Equipment Equipment { get; }
        public EquipmentSlot Slot { get; }

        public UnEquipItemEvent(Equipment equipment, EquipmentSlot slot) {
            Equipment = equipment;
            Slot = slot;
        }
    }
}
