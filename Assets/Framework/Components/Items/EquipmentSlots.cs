using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EquipmentSlots : GenericContainer<EquipmentSlot>,  IReceive<ContainerStatusChanged> {
        public EquipmentSlots(IList<EquipmentSlot> values) : base(values) {}

        public EquipmentSlot GetSlot(int slotType) {
            for (int i = 0; i < List.Count; i++) {
                if (List[i].SlotIsCompatible(slotType)) {
                    return List[i];
                }
            }
            return null;
        }

        public EquipmentSlot GetSlotExact(int slotType) {
            for (int i = 0; i < List.Count; i++) {
                if (List[i].TargetSlot == slotType) {
                    return List[i];
                }
            }
            return null;
        }

        public bool TryEquip(Entity entity) {
            var equip = entity.Get<Equipment>();
            if (equip == null) {
                return false;
            }
            for (int i = 0; i < List.Count; i++) {
                if (List[i].SlotIsCompatible(equip.EquipmentSlotType) && List[i].EquipItem(entity)) {
                    return true;
                }
            }
            return false;
        }

        public override void Add(EquipmentSlot slot){
            base.Add(slot);
            if (slot != null) {
                slot.SlotOwner = this;
            }
        }

        public void Handle(ContainerStatusChanged arg) {
            for (int i = 0; i < List.Count; i++) {
                List[i].Handle(arg);
            }
        }
    }

    public struct EquipmentChanged : IEntityMessage {
        public Entity Owner;

        public EquipmentChanged(Entity owner) {
            Owner = owner;
        }
    }
}
