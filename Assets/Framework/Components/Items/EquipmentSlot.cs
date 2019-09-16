using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public interface IEquipmentHolder {
        System.Action<Entity> OnItemChanged { get; set; }
        string TargetSlot { get; }
        string LastEquipStatus { get; }
        Entity Item { get; }
        bool AddItem(Entity item);
        Transform EquipTr { get; }
    }

    [System.Serializable]
    public class EquipmentSlot: IEquipmentHolder, ISerializable {

        public Action<Entity> OnItemChanged { get;set; }

        private string _targetSlot;
        private CachedTransform _equipTr;
        private CachedEntity _item = new CachedEntity();
        private CachedComponent<EquipmentSlots> _slots;
        private CachedComponent<Equipment> _equipment = new CachedComponent<Equipment>();
        private string _lastEquipStatus = "";
        
        public EquipmentSlot(EquipmentSlots owner, string targetSlot, string name, Transform equipTr) {
            _targetSlot = targetSlot;
            _equipTr = new CachedTransform(equipTr);
            Name = name;
            _slots = new CachedComponent<EquipmentSlots>(owner);
        }

        public EquipmentSlot(SerializationInfo info, StreamingContext context) {
            _targetSlot = info.GetValue(nameof(_targetSlot), _targetSlot);
            _equipTr = info.GetValue(nameof(_equipTr), _equipTr);
            _item = info.GetValue(nameof(_item), _item);
            _slots = info.GetValue(nameof(_slots), _slots);
            _equipment = info.GetValue(nameof(_equipment), _equipment);
            _lastEquipStatus = info.GetValue(nameof(_lastEquipStatus), _lastEquipStatus);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_targetSlot), _targetSlot);
            info.AddValue(nameof(_equipTr), _equipTr);
            info.AddValue(nameof(_item), _item);
            info.AddValue(nameof(_slots), _slots);
            info.AddValue(nameof(_equipment), _equipment);
            info.AddValue(nameof(_lastEquipStatus), _lastEquipStatus);
        }

        public EquipmentSlots SlotOwner { get { return _slots.Value; } }
        public Equipment CurrentEquipment { get { return _equipment.Value; } }
        public Entity Item { get { return _item; } }
        public string LastEquipStatus { get { return _lastEquipStatus; } set { _lastEquipStatus = value; } }
        public string TargetSlot { get { return _targetSlot; } }
        public string Name { get; }
        public Transform EquipTr { get => _equipTr; }

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
                _lastEquipStatus = "Already equipped";
                return false;
            }
            var equip = item.Get<Equipment>();
            if (equip == null || !SlotIsCompatible(equip.EquipmentSlotType)) {
                _lastEquipStatus = "Wrong type";
                return false;
            }
            InventoryItem containerItem = item.Get<InventoryItem>();
            if (containerItem == null) {
                return false;
            }
            if (Item != null) {
                if (Player.MainInventory.IsFull) {
                    _lastEquipStatus = "Owner inventory full";
                    return false;
                }
                var oldItem = Item;
                ClearEquippedItem(true);
                if (oldItem != null) {
                    Player.MainInventory.TryAdd(oldItem);
                }
            }
            _item.Set(item);
            if (containerItem.Inventory != null) {
                containerItem.Inventory.Remove(item);
            }
            var owner = SlotOwner.GetEntity();
            item.ParentId = owner;
            equip.Equip(this);
            _equipment.Set(item);
            SetStats();
            containerItem.SetContainer(SlotOwner);
            if (OnItemChanged != null) {
                OnItemChanged(_item);
            }
            var msg = new EquipmentChanged(owner, this);
            item.Post(msg);
            owner.Post(msg);
            _lastEquipStatus = "";
            return true;
        }

        protected virtual void SetStats() {}
        protected virtual void ClearStats(){}

        //public float RecoveryPenalty() {
        //    if (_item == null) {
        //        return 0;
        //    }
        //    if (_targetSlot.IsWeaponSlot()) {
        //        return 0;
        //    }
        //    return _owner.PlayerStats.GetRecoveryAdjustments(_item, _item.Skill, _item.Weight);
        //}


        public virtual bool SlotIsCompatible(string slotType) {
            return slotType == _targetSlot;
        }
        
        public bool RemoveItemAddToOwnInventory() {
            var item = Item;
            ClearEquippedItem(false);
            if (item != null) {
                Player.MainInventory.Add(item);
            }
            return true;
        }

        private void ClearEquippedItem(bool isSwap) {
            if (Item != null) {
                var item = Item;
                var owner = SlotOwner.GetEntity();
                ClearStats();
                item.ClearParent(owner);
                if (CurrentEquipment != null) {
                    CurrentEquipment.UnEquip();                    
                }
                var container = item.Get<InventoryItem>();
                if (container != null && container.Inventory == SlotOwner) {
                    container.SetContainer(null);
                }
                item.Post(new EquipmentChanged(null, null));
                if (!isSwap) {
                    owner.Post(new EquipmentChanged(owner, this));
                    OnItemChanged?.Invoke(null);
                }
            }
            _equipment.Clear();
        }

        public void ClearContents() {
            ClearEquippedItem(false);
        }
    }

    public abstract class StatModHolder {
        
        public abstract void Attach(BaseStat target);
        public abstract void Remove();
        public abstract string StatID { get;}
    }

    public class BasicValueModHolder : StatModHolder {
        private string _id;
        private BaseStat _targetStat;
        private float _amount;

        public override string StatID { get { return _targetStat != null ? _targetStat.ID : ""; } }

        public BasicValueModHolder(float amount) {
            _amount = amount;
        }

        public override void Attach(BaseStat target) {
            if (target == null) {
                return;
            }
            _targetStat = target;
            _id = target.AddValueMod(_amount);
        }

        public override void Remove() {
            if (_targetStat != null) {
                _targetStat.RemoveMod(_id);
            }
            _targetStat = null;
            _id = "";
        }
    }

    public class BasicPercentModHolder : StatModHolder {
        private string _id;
        private BaseStat _targetStat;
        private float _percent;

        public override string StatID { get { return _targetStat != null ? _targetStat.ID : ""; } }

        public BasicPercentModHolder(float percent) {
            _percent = percent;
        }

        public override void Attach(BaseStat target) {
            if (target == null) {
                return;
            }
            _targetStat = target;
            _id = target.AddPercentMod(_percent);
        }

        public override void Remove() {
            if (_targetStat != null) {
                _targetStat.RemoveMod(_id);
            }
            _targetStat = null;
            _id = "";
        }
    }

    public class DerivedStatModHolder : StatModHolder {

        private string _id;
        private BaseStat _sourceStat;
        private float _percent;

        public override string StatID { get { return _sourceStat != null ? _sourceStat.ID : ""; } }

        public DerivedStatModHolder(BaseStat source, float percent) {
            _sourceStat = source;
            _percent = percent;
        }

        public DerivedStatModHolder(BaseStat source, BaseStat target, float percent) {
            _sourceStat = source;
            _percent = percent;
            Attach(target);
        }

        public override void Attach(BaseStat target) {
            if (target == null) {
                return;
            }
            _id = _sourceStat.AddDerivedStat(_percent, target);
        }

        public override void Remove() {
            if (_sourceStat == null) {
                return;
            }
            _sourceStat.RemoveDerivedStat(_id);
            _sourceStat = null;
        }
    }
}