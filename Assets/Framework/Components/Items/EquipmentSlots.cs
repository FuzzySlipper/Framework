using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class EquipmentSlots : IComponent, IReceive<ContainerStatusChanged> {

        private GenericContainer<EquipmentSlot> _list = new GenericContainer<EquipmentSlot>();
        
        public EquipmentSlots() {}
        public EquipmentSlots(SerializationInfo info, StreamingContext context) {
            _list = info.GetValue(nameof(_list), _list);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_list), _list);
        }

        public int Count { get { return _list.Count; } }
        public EquipmentSlot this[int index] { get { return _list[index]; } }
        
        public EquipmentSlot GetSlot(string slotType) {
            for (int i = 0; i < _list.Count; i++) {
                if (_list[i].SlotIsCompatible(slotType)) {
                    return _list[i];
                }
            }
            return null;
        }

        public EquipmentSlot GetSlotExact(string slotType) {
            for (int i = 0; i < _list.Count; i++) {
                if (_list[i].TargetSlot == slotType) {
                    return _list[i];
                }
            }
            return null;
        }

        public EquipmentSlot GetSlotNameExact(string slotName) {
            for (int i = 0; i < _list.Count; i++) {
                if (_list[i].Name == slotName) {
                    return _list[i];
                }
            }
            return null;
        }

        public bool TryEquip(Entity entity) {
            var equip = entity.Get<Equipment>();
            if (equip == null) {
                return false;
            }
            if (TryEquip(equip, entity, false)) {
                return true;
            }
            if (TryEquip(equip, entity, true)) {
                return true;
            }
            return false;
        }

        private bool TryEquip(Equipment equip, Entity entity, bool overrideCurrent) {
            for (int i = 0; i < _list.Count; i++) {
                if (_list[i].Item != null && !overrideCurrent) {
                    continue;
                }
                if (_list[i].SlotIsCompatible(equip.EquipmentSlotType) && _list[i].EquipItem(entity)) {
                    return true;
                }
            }
            return false;
        }

        public void Add(string targetSlot, string name, Transform equipTr){
            _list.Add(new EquipmentSlot(this, targetSlot, name, equipTr));
        }

        public void Handle(ContainerStatusChanged arg) {
            for (int i = 0; i < _list.Count; i++) {
                _list[i].Handle(arg);
            }
        }
    }

    public struct EquipmentChanged : IEntityMessage {
        public Entity Owner;
        public IEquipmentHolder Slot;

        public EquipmentChanged(Entity owner, IEquipmentHolder slot) {
            Owner = owner;
            Slot = slot;
        }
    }
}
