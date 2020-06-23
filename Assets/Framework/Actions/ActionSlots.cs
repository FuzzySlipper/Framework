using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class ActionSlots : IComponent, IEntityContainer {

        private GenericContainer<ActionSlot> _list = new GenericContainer<ActionSlot>();
        
<<<<<<< HEAD
        public ActionSlots() { }
=======
        public ActionSlots(int amountPrimary, int amountSecondary, int amtHidden) {
            AddSlot(ActionPivotTypes.Primary, amountPrimary);
            AddSlot(ActionPivotTypes.Secondary, amountSecondary);
            AddSlot(ActionPivotTypes.Hidden, amtHidden);
        }
        
        public ActionSlots(){}
>>>>>>> FirstPersonAction

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

        public void AddSlot(ActionSlot slot) {
            _list.Add(slot);
        }

<<<<<<< HEAD
=======
        public void AddSlot(string slotType, int cnt) {
            for (int i = 0; i <= cnt; i++) {
                _list.Add(new ActionSlot(this, slotType));
            }
        }

>>>>>>> FirstPersonAction
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
<<<<<<< HEAD
                if (_list[i].Type != PivotTypes.Hidden ) {
=======
                if (_list[i].TargetSlot != ActionPivotTypes.Hidden ) {
>>>>>>> FirstPersonAction
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
        private CachedComponent<ActionSlots> _owner;
        private BaseActionTemplate _action;
        
        public Entity Owner { get { return _owner.Value.GetEntity(); } }
        public string[] CompatibleSlots { get; }
        public Type[] RequiredTypes { get; }
        public List<StatModHolder> CurrentStats { get; }
        public IEntityContainer Container { get { return _owner.Value; } }
        public BaseActionTemplate Action { get { return _action; } }
        public string LastEquipStatus { get; set; }
        public string TargetSlot { get; }
        public Transform EquipTr { get { return null; } }
<<<<<<< HEAD
        public string Type { get; }
=======
>>>>>>> FirstPersonAction
        public Entity Item {
            get {
                return _cachedItem.Entity;
            }
            set {
                // if (value == null) {
                //     if (Action?.Config.EquippedSlot >= 0) {
                //         Owner.Get<ReadyActions>().RemoveAction(Action.Config.EquippedSlot);
                //     }
                // }
                _cachedItem.Set(value);
                if (value != null) {
                    _action = value.GetTemplate<BaseActionTemplate>();
                }
            }
        }

        public ActionSlot(ActionSlots slotOwner, string type) {
            _owner = new CachedComponent<ActionSlots>(slotOwner);
<<<<<<< HEAD
            Type = type;
=======
            TargetSlot = type;
>>>>>>> FirstPersonAction
            CompatibleSlots = null;
            CurrentStats = null;
            RequiredTypes = new[] {typeof(ActionConfig)};
        }

        public ActionSlot(SerializationInfo info, StreamingContext context) {
<<<<<<< HEAD
            Type = info.GetValue(nameof(Type), Type);
=======
>>>>>>> FirstPersonAction
            _cachedItem = info.GetValue(nameof(_cachedItem), _cachedItem);
            _owner = info.GetValue(nameof(_owner), _owner);
            _action = info.GetValue(nameof(_action), _action);
            LastEquipStatus = info.GetValue(nameof(LastEquipStatus), LastEquipStatus);
            CompatibleSlots = null;
            CurrentStats = null;
            RequiredTypes = new[] {typeof(ActionConfig)};
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
<<<<<<< HEAD
            info.AddValue(nameof(Type), Type);
=======
>>>>>>> FirstPersonAction
            info.AddValue(nameof(_cachedItem), _cachedItem);
            info.AddValue(nameof(_owner), _owner);
            info.AddValue(nameof(_action), _action);
            info.AddValue(nameof(LastEquipStatus), LastEquipStatus);
        }

        public bool FinalCheck(Entity item, out string error) {
            var action = item.Get<ActionConfig>();
<<<<<<< HEAD
            if (action.Type != Type) {
                error = string.Format("Requires {0} Slot", Type);
=======
            if (action.TargetSlot != TargetSlot) {
                error = string.Format("Requires {0} Slot", TargetSlot);
>>>>>>> FirstPersonAction
                return false;
            }
            error = null;
            return true;
        }
    }
}
