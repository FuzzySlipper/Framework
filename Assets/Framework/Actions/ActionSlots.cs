using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class ActionSlots : IComponent, IEntityContainer {

        private GenericContainer<ActionSlot> _list = new GenericContainer<ActionSlot>();
        
        public ActionSlots(int amountPrimary, int amountSecondary, int amtHidden) {
            for (int i = 0; i <= amountPrimary; i++) {
                _list.Add(new ActionSlot(this, false, false));
            }
            for (int i = 0; i <= amountSecondary; i++) {
                _list.Add(new ActionSlot(this, true,false));
            }
            for (int i = 0; i <= amtHidden; i++) {
                _list.Add(new ActionSlot(this, true, true));
            }
        }

        public ActionSlots(SerializationInfo info, StreamingContext context) {
            _list = info.GetValue(nameof(_list), _list);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_list), _list);
        }

        public int Count { get { return _list.Count; } }
        public Entity this[int index] { get { return _list[index].Item; } }
        public Entity Owner { get { return this.GetEntity(); } }

        public ActionSlot GetSlot(int slot) {
            return _list[slot];
        }

        public int ContainerSystemAdd(Entity item) {
            if (EquipToEmpty(item)) {
                for (int i = 0; i < _list.Count; i++) {
                    if (_list[i].Item == item) {
                        return i;
                    }
                }
            }
            return -1;
        }

        public bool IsFull { get { return false; } }
        public bool Contains(Entity item) {
            for (int i = 0; i < Count; i++) {
                if (this[i] == item) {
                    return true;
                }
            }
            return false;
        }

        public void ContainerSystemSet(Entity item, int index) {
            World.Get<EquipmentSystem>().TryEquip(_list[index], item);
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
        
        public bool EquipToEmpty(Entity actionEntity) {
            for (int i = 0; i < _list.Count; i++) {
                if (_list[i].Item == null && World.Get<EquipmentSystem>().TryEquip(_list[i], actionEntity)) {
                    return true;
                }
            }
            return false;
        }

        public bool EquipToHidden(Entity actionEntity) {
            for (int i = 0; i < _list.Count; i++) {
                if (!_list[i].IsHidden) {
                    continue;
                }
                if (_list[i].Item == null && World.Get<EquipmentSystem>().TryEquip(_list[i], actionEntity)) {
                    return true;
                }
            }
            return false;
        }
    }

    public class ActionSlot : IEquipmentHolder, ISerializable {
        public System.Action<Entity> OnItemChanged { get; set; }
        
        private CachedEntity _cachedItem = new CachedEntity();
        private bool _isSecondary;
        private bool _isHidden;
        private CachedComponent<ActionSlots> _owner;
        private CachedComponent<Action> _action = new CachedComponent<Action>();
        
        public Entity Owner { get { return _owner.Value.GetEntity(); } }
        public string[] CompatibleSlots { get; }
        public Type[] RequiredTypes { get; }
        public List<StatModHolder> CurrentStats { get; }
        public IEntityContainer Container { get { return _owner.Value; } }
        public Action Action { get { return _action.Value; } }
        public string LastEquipStatus { get; set; }
        public string TargetSlot { get { return "Usable"; } }
        public Transform EquipTr { get { return null; } }
        public bool IsSecondary { get => _isSecondary; }
        public bool IsHidden { get => _isHidden; }
        public Entity Item {
            get {
                return _cachedItem.Entity;
            }
            set {
                if (value == null) {
                    if (Action?.EquippedSlot >= 0) {
                        Owner.Get<CurrentActions>().RemoveAction(Action.EquippedSlot);
                    }
                }
                _cachedItem.Set(value);
                _action.Set(value);
            }
        }

        public ActionSlot(ActionSlots slotOwner, bool isSecondary, bool isHidden) {
            _owner = new CachedComponent<ActionSlots>(slotOwner);
            _isSecondary = isSecondary;
            _isHidden = isHidden;
            CompatibleSlots = null;
            CurrentStats = null;
            RequiredTypes = new[] {typeof(Action)};
        }

        public ActionSlot(SerializationInfo info, StreamingContext context) {
            _isSecondary = info.GetValue(nameof(_isSecondary), _isSecondary);
            _isHidden = info.GetValue(nameof(_isHidden), _isHidden);
            _cachedItem = info.GetValue(nameof(_cachedItem), _cachedItem);
            _owner = info.GetValue(nameof(_owner), _owner);
            _action = info.GetValue(nameof(_action), _action);
            LastEquipStatus = info.GetValue(nameof(LastEquipStatus), LastEquipStatus);
            CompatibleSlots = null;
            CurrentStats = null;
            RequiredTypes = new[] {typeof(Action)};
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_isSecondary), _isSecondary);
            info.AddValue(nameof(_isHidden), _isHidden);
            info.AddValue(nameof(_cachedItem), _cachedItem);
            info.AddValue(nameof(_owner), _owner);
            info.AddValue(nameof(_action), _action);
            info.AddValue(nameof(LastEquipStatus), LastEquipStatus);
        }

        public bool FinalCheck(Entity item, out string error) {
            var action = item.Get<Action>();
            if (action.Primary && IsSecondary) {
                error = "Requires Primary Slot";
                return false;
            }
            if (!action.Primary && !IsSecondary) {
                error = "Requires Secondary Slot";
                return false;
            }
            error = null;
            return true;
        }
    }
}
