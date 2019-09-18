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

        public bool Add(Entity item) {
            return EquipToEmpty(item, item.Get<Action>());
        }

        public bool Remove(Entity entity) {
            for (int i = 0; i < Count; i++) {
                if (this[i] == entity) {
                    _list[i].ClearContents();
                    return true;
                }
            }
            return false;
        }

        public void Clear() {
            for (int i = 0; i < _list.Count; i++) {
                _list[i].ClearContents();
            }
        }
        
        public bool EquipToEmpty(Entity actionEntity, Action action) {
            for (int i = 0; i < _list.Count; i++) {
                if (_list[i].Item == null && _list[i].EquipItem(actionEntity, action)) {
                    return true;
                }
            }
            return false;
        }

        public bool EquipToHidden(Entity actionEntity, Action action) {
            for (int i = 0; i < _list.Count; i++) {
                if (!_list[i].IsHidden) {
                    continue;
                }
                if (_list[i].Item == null && _list[i].EquipItem(actionEntity, action)) {
                    return true;
                }
            }
            return false;
        }
    }

    public class ActionSlot : IEquipmentHolder, ISerializable {
        public System.Action<Entity> OnItemChanged { get; set; }
        
        private CachedEntity _cachedItem = new CachedEntity();
        private string _lastEquipStatus = "";
        private bool _isSecondary;
        private bool _isHidden;
        private CachedComponent<ActionSlots> _owner;
        private CachedComponent<Action> _action = new CachedComponent<Action>();

        public ActionSlots SlotOwner { get { return _owner.Value; } }
        public Entity Item { get { return _cachedItem.Entity; } }
        public Action Action { get { return _action.Value; } }
        public string LastEquipStatus { get { return _lastEquipStatus; } }
        public string TargetSlot { get { return "Usable"; } }
        public Transform EquipTr { get { return null; } }
        public bool IsSecondary { get => _isSecondary; }
        public bool IsHidden { get => _isHidden; }

        public ActionSlot(ActionSlots slotOwner, bool isSecondary, bool isHidden) {
            _owner = new CachedComponent<ActionSlots>(slotOwner);
            _isSecondary = isSecondary;
            _isHidden = isHidden;
        }

        public ActionSlot(SerializationInfo info, StreamingContext context) {
            _isSecondary = info.GetValue(nameof(_isSecondary), _isSecondary);
            _isHidden = info.GetValue(nameof(_isHidden), _isHidden);
            _cachedItem = info.GetValue(nameof(_cachedItem), _cachedItem);
            _owner = info.GetValue(nameof(_owner), _owner);
            _action = info.GetValue(nameof(_action), _action);
            _lastEquipStatus = info.GetValue(nameof(_lastEquipStatus), _lastEquipStatus);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_isSecondary), _isSecondary);
            info.AddValue(nameof(_isHidden), _isHidden);
            info.AddValue(nameof(_cachedItem), _cachedItem);
            info.AddValue(nameof(_owner), _owner);
            info.AddValue(nameof(_action), _action);
            info.AddValue(nameof(_lastEquipStatus), _lastEquipStatus);
        }
        public bool AddItem(Entity item) {
            if (item == null) {
                _lastEquipStatus = "Item null";
                return false;
            }
            var action = item.Get<Action>();
            if (action == null) {
                _lastEquipStatus = "Wrong type";
                return false;
            }
            return EquipItem(item, action);
        }

        public bool EquipItem(Entity actionEntity, Action action) {
            if (action == null || _isSecondary && action.Primary || !_isSecondary && !action.Primary) {
                _lastEquipStatus = "Wrong type";
                return false;
            }
            if (actionEntity == Item) {
                return false;
            }
            InventoryItem containerItem = actionEntity.Get<InventoryItem>();
            if (containerItem == null) {
                return false;
            }
            //var req = item.Get<SkillRequirement>();
            //if (req != null && (int) _owner.Stats.Get<SkillStat>(req.Skill).CurrentRank < 
            //    (int) req.Required) {
            //    _lastEquipStatus = "Skill too low";
            //    return false;
            //}
            if (Item != null) {
                RemoveItemAddToOwnInventory();
            }
            if (containerItem.Inventory != null) {
                containerItem.Inventory.Remove(actionEntity);
            }
            _action.Set(action);
            _cachedItem.Set(actionEntity);
            var owner = SlotOwner.GetEntity();
            actionEntity.ParentId = owner;
            containerItem.SetContainer(SlotOwner);
            var msg = new EquipmentChanged(owner, this);
            actionEntity.Post(msg);
            if (OnItemChanged != null) {
                OnItemChanged(Item);
            }
            _lastEquipStatus = "";
            return true;
        }

        public bool RemoveItemAddToOwnInventory() {
            var item = Item;
            ClearContents();
            if (item != null) {
                SlotOwner.GetEntity().Get<ItemInventory>()?.Add(item);
            }
            return true;
        }

        public void ClearContents() {
            if (Item != null) {
                if (Action?.EquippedSlot >= 0) {
                    SlotOwner.GetEntity().Get<CurrentActions>().RemoveAction(Action.EquippedSlot);
                }
                var container = Item.Get<InventoryItem>();
                if (container != null && container.Inventory == SlotOwner) {
                    container.SetContainer(null);
                }
                _action.Clear();
                _cachedItem.Clear();
            }
            if (OnItemChanged != null) {
                OnItemChanged(null);
            }
        }
    }
}
