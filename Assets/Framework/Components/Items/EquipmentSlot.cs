using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IEquipmentHolder {
        System.Action<Entity> OnItemChanged { get; set; }
        string TargetSlot { get; }
        string LastEquipStatus { get; }
        Entity Item { get; }
        bool AddItem(Entity item);
    }

    public class EquipmentSlot: IEquipmentHolder {

        public EquipmentSlots SlotOwner;
        public Action<Entity> OnItemChanged { get;set; }

        private string _targetSlot;
        private Transform _equipTr;

        public EquipmentSlot(string targetSlot, string name, Transform equipTr) {
            _targetSlot = targetSlot;
            _equipTr = equipTr;
            Name = name;
        }

        private Entity _item;
        protected string _lastEquipStatus = "";
        
        public Equipment CurrentEquipment { get; private set; }
        public Entity Item { get { return _item; } }
        public string LastEquipStatus { get { return _lastEquipStatus; } }
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
            var msg = new EquipmentChanged(owner, this);
            _item.Post(msg);
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
            var item = _item;
            ClearEquippedItem(false);
            if (item != null) {
                Player.MainInventory.Add(item);
            }
            return true;
        }

        private void ClearEquippedItem(bool isSwap) {
            if (_item != null) {
                ClearStats();
                _item.ClearParent(SlotOwner.Owner);
                _item.Get<Equipment>().UnEquip();
                _item.RemoveObserver(SlotOwner);
                _item.Post(new EquipmentChanged(null, null));
                _item = null;
                if (!isSwap) {
                    var owner = SlotOwner.GetEntity();
                    owner.Post(new EquipmentChanged(owner, this));
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