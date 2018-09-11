using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EquipmentSlot {

        public EquipmentSlots SlotOwner;
        public Action<Entity> OnItemChanged;

        private int _targetSlot;

        public EquipmentSlot(int targetSlot) {
            _targetSlot = targetSlot;
        }

        private Entity _item;
        private string _lastEquipStatus = "";
        private List<StatModHolder> _currentStats = new List<StatModHolder>();
        
        public Equipment CurrentEquipment { get; private set; }
        public Entity Item { get { return _item; } }
        public string LastEquipStatus { get { return _lastEquipStatus; } }
        public int TargetSlot { get { return _targetSlot; } }
        public string Name {get { return EquipSlotType.GetDescriptionAt(_targetSlot);} }

        public bool AddItem(Entity item) {
            if (item == null) {
                _lastEquipStatus = "Item null";
                return false;
            }
            var equip = item.Get<Equipment>();
            if (equip == null) {
                _lastEquipStatus = "Wrong class";
                return false;
            }
            return EquipItem(item);
        }

        public virtual bool EquipItem(Entity item) {
            if (item == Item) {
                return false;
            }
            var equip = item.Get<Equipment>();
            if (equip == null || !SlotIsCompatible(equip.EquipmentSlotType)) {
                _lastEquipStatus = "Wrong type";
                return false;
            }
            if (_item != null) {
                if (Player.MainInventory.IsFull) {
                    _lastEquipStatus = "Owner inventory full";
                    return false;
                }
                var oldItem = _item;
                ClearEquippedItem(true);
                if (oldItem != null) {
                    Player.MainInventory.TryAdd(oldItem);
                }
            }
            _item = item;
            item.Get<InventoryItem>(i => i.Inventory?.Remove(item));
            _item.ParentId = SlotOwner.Owner;
            equip.Equip(this);
            CurrentEquipment = equip;
            SetStats();
            item.AddObserver(SlotOwner);
            if (OnItemChanged != null) {
                OnItemChanged(_item);
            }
            var owner = SlotOwner.GetEntity();
            var msg = new EquipmentChanged(owner);
            _item.Post(msg);
            owner.Post(msg);
            _lastEquipStatus = "";
            return true;
        }

        protected virtual void SetStats() {}

        private void ClearStats() {
            for (int i = 0; i < _currentStats.Count; i++) {
                _currentStats[i].Remove();
            }
            _currentStats.Clear();
        }

        //public float RecoveryPenalty() {
        //    if (_item == null) {
        //        return 0;
        //    }
        //    if (_targetSlot.IsWeaponSlot()) {
        //        return 0;
        //    }
        //    return _owner.PlayerStats.GetRecoveryAdjustments(_item, _item.Skill, _item.Weight);
        //}


        public virtual bool SlotIsCompatible(int slotType) {
            return slotType == _targetSlot;
        }
        
        public bool RemoveItemAddToOwnInventory() {
            var item = _item;
            ClearEquippedItem(false);
            if (item != null) {
                Player.MainInventory.Add(item);
            }
            return true;
        }

        private void ClearEquippedItem(bool isSwap) {
            ClearStats();
            if (_item != null) {
                _item.ClearParent(SlotOwner.Owner);
                _item.Get<Equipment>().Unequip();
                _item.RemoveObserver(SlotOwner);
                _item.Post(new EquipmentChanged(null));
                _item = null;
                if (!isSwap) {
                    var owner = SlotOwner.GetEntity();
                    owner.Post(new EquipmentChanged(owner));
                    OnItemChanged?.Invoke(null);
                }
            }
            CurrentEquipment = null;
        }

        public void ClearContents() {
            ClearEquippedItem(false);
        }

        public void Handle(ContainerStatusChanged arg) {
            if (arg.Entity == _item) {
                ClearEquippedItem(false);
            }
        }
    }

    public struct StatModHolder {
        private string _id;
        private BaseStat _stat;

        public StatModHolder(BaseStat source, BaseStat target, float percent) {
            _stat = source;
            _id = _stat.AddDerivedStat(percent, target);
        }

        public void Remove() {
            if (_stat == null) {
                return;
            }
            _stat.RemoveDerivedStat(_id);
            _stat = null;
        }
    }
}