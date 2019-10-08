using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class EquipmentSlots : IComponent, IEntityContainer {

        private GenericContainer<EquipmentSlot> _list = new GenericContainer<EquipmentSlot>();
        
        public EquipmentSlots() {}
        public EquipmentSlots(SerializationInfo info, StreamingContext context) {
            _list = info.GetValue(nameof(_list), _list);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_list), _list);
        }

        public int Count { get { return _list.Count; } }
        public Entity this[int index] { get { return _list[index].Item; } }
        public Entity Owner { get { return this.GetEntity(); } }
        public bool IsFull { get; }
        public bool Contains(Entity item) {
            for (int i = 0; i < _list.Count; i++) {
                if (_list[i].Item == item) {
                    return true;
                }
            }
            return false;
        }

        public void ContainerSystemSet(Entity item, int index) {
            World.Get<EquipmentSystem>().TryEquip(_list[index], item);
        }

        public int ContainerSystemAdd(Entity item) {
            if (TryEquip(item)) {
                for (int i = 0; i < _list.Count; i++) {
                    if (_list[i].Item == item) {
                        return i;
                    }
                }
            }
            return 0;
        }

        public bool Remove(Entity entity) {
            for (int i = 0; i < Count; i++) {
                if (this[i] == entity) {
                    World.Get<EquipmentSystem>().ClearEquippedItem(_list[i], false);
                    return true;
                }
            }
            return false;
        }

        public void Clear() {
            for (int i = 0; i < _list.Count; i++) {
                World.Get<EquipmentSystem>().ClearEquippedItem(_list[i], false);
            }
        }

        public EquipmentSlot GetSlot(string slotType) {
            for (int i = 0; i < _list.Count; i++) {
                if (_list[i].CompatibleSlots.Contains(slotType)) {
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
                if (_list[i].CompatibleSlots.Contains(equip.EquipmentSlotType) && World.Get<EquipmentSystem>().TryEquip(_list[i], entity)) {
                    return true;
                }
            }
            return false;
        }

        public void Add(string targetSlot, string name, Transform equipTr){
            _list.Add(new EquipmentSlot(this, targetSlot, name, equipTr));
        }
    }

    public struct EquipmentChanged : IEntityMessage {
        public Entity Owner { get; }
        public IEquipmentHolder Slot { get; }

        public EquipmentChanged(Entity owner, IEquipmentHolder slot) {
            Owner = owner;
            Slot = slot;
        }
    }
}
