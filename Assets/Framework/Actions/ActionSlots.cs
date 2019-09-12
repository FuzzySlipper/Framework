using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ActionSlots : GenericContainer<ActionSlot> {
        public ActionSlots(int amountPrimary, int amountSecondary, int amtHidden) {
            for (int i = 0; i <= amountPrimary; i++) {
                Add(new ActionSlot(this, false, false));
            }
            for (int i = 0; i <= amountSecondary; i++) {
                Add(new ActionSlot(this, true,false));
            }
            for (int i = 0; i <= amtHidden; i++) {
                Add(new ActionSlot(this, true, true));
            }
        }

        public bool EquipToEmpty(Entity actionEntity, Action action) {
            for (int i = 0; i < Count; i++) {
                if (this[i].Item == null && this[i].EquipItem(actionEntity, action)) {
                    return true;
                }
            }
            return false;
        }

        public bool EquipToHidden(Entity actionEntity, Action action) {
            for (int i = 0; i < Count; i++) {
                if (!this[i].IsHidden) {
                    continue;
                }
                if (this[i].Item == null && this[i].EquipItem(actionEntity, action)) {
                    return true;
                }
            }
            return false;
        }
    }

    public class ActionSlot : IReceive<ContainerStatusChanged>, IReceive<EntityDestroyed>, IReceive<EquipmentChanged>, IEquipmentHolder {
        public System.Action<Entity> OnItemChanged { get; set; }
        public ActionSlots SlotOwner;

        private Entity _item;
        private string _lastEquipStatus = "";
        private bool _isSecondary;
        private bool _isHidden;

        public Entity Item { get { return _item; } }
        public Action Action { get; private set; }
        public string LastEquipStatus { get { return _lastEquipStatus; } }
        public string TargetSlot { get { return "Usable"; } }
        public Transform EquipTr { get { return null; } }
        public bool IsSecondary { get => _isSecondary; }
        public bool IsHidden { get => _isHidden; }

        public ActionSlot(ActionSlots slotOwner, bool isSecondary, bool isHidden) {
            SlotOwner = slotOwner;
            _isSecondary = isSecondary;
            _isHidden = isHidden;
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
            //var req = item.Get<SkillRequirement>();
            //if (req != null && (int) _owner.Stats.Get<SkillStat>(req.Skill).CurrentRank < 
            //    (int) req.Required) {
            //    _lastEquipStatus = "Skill too low";
            //    return false;
            //}
            if (_item != null) {
                RemoveItemAddToOwnInventory();
            }
            Action = action;
            _item = actionEntity;
            actionEntity.Get<InventoryItem>(i => i.Inventory?.Remove(actionEntity));
            var owner = SlotOwner.GetEntity();
            _item.ParentId = owner;
            var msg = new EquipmentChanged(owner, this);
            _item.Post(msg);
            _item.AddObserver(this);
            if (OnItemChanged != null) {
                OnItemChanged(Item);
            }
            _lastEquipStatus = "";
            return true;
        }

        public bool RemoveItemAddToOwnInventory() {
            var item = _item;
            ClearEquippedItem();
            if (item != null) {
                SlotOwner.Get<ItemInventory>().Add(item);
            }
            return true;
        }

        private void ClearEquippedItem() {
            if (_item != null) {
                if (Action.EquippedSlot >= 0) {
                    SlotOwner.Get<CurrentActions>().RemoveAction(Action.EquippedSlot);
                }
                Action = null;
                _item.RemoveObserver(this);
                _item = null;
            }
            if (OnItemChanged != null) {
                OnItemChanged(null);
            }
        }

        public void Handle(ContainerStatusChanged arg) {
            if (arg.Entity == _item && arg.EntityContainer != null) {
                ClearEquippedItem();
            }
        }

        public void Handle(EntityDestroyed arg) {
            if (arg.Entity == _item) {
                ClearEquippedItem();
            }
        }

        public void Handle(EquipmentChanged arg) {
            if (arg.Slot != this) {
                ClearEquippedItem();
            }
        }
    }
}
