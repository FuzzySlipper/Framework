using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class EquipmentSystem : SystemBase, IReceiveGlobal<SaveGameLoaded>, IReceive<DataDescriptionUpdating>,
        IReceive<TooltipDisplaying> {

        
        public EquipmentSystem() {
            NodeFilter<EquipmentNode>.Setup(EquipmentNode.GetTypes());
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(Equipment)
            }));
        }

        public bool TryEquip(IEquipmentHolder holder, Entity item) {
            if (item == null || holder == null) {
                return false;
            }
            if (item == holder.Item) {
                holder.LastEquipStatus = "Already equipped";
                return false;
            }
            for (int i = 0; i < holder.RequiredTypes.Length; i++) {
                if (!item.HasReference(holder.RequiredTypes[i])) {
                    holder.LastEquipStatus = "Wrong type";
                    return false;
                }
            }
            if (!holder.FinalCheck(item, out var errorMsg)) {
                holder.LastEquipStatus = errorMsg;
                return false;
            }
            var equipNode = item.GetNode<EquipmentNode>();
            if (equipNode == null) {
                holder.LastEquipStatus = "Item is bugged";
                return false;
            }
            if (equipNode.Item == null) {
                holder.LastEquipStatus = "No Inventory Slot";
                return false;
            }
            var owner = holder.Owner;
            if (owner == null) {
                holder.LastEquipStatus = "Holder has no owner";
                return false;
            }
            if (holder.CompatibleSlots != null && (equipNode.Equipment == null || !holder.CompatibleSlots.Contains(equipNode.Equipment.EquipmentSlotType))) {
                holder.LastEquipStatus = "Wrong Slot";
                return false;
            }
            if (equipNode.SkillRequirement != null) {
                var skill = owner.Get<StatsContainer>().Get<SkillStat>(equipNode.SkillRequirement.Skill);
                if (skill != null && skill.CurrentRank < equipNode.SkillRequirement.Required) {
                    holder.LastEquipStatus = "Skill too low";
                    return false;
                }
            }
            if (holder.Item != null) {
                if (!ClearAndAddToInventory(holder, true)) {
                    return false;
                }
            }
            holder.Item = item;
            equipNode.Item.Inventory?.Remove(item);
            item.ParentId = owner;
            SetStats(holder, equipNode);
            equipNode.Item.SetContainer(holder.Container);
            holder.OnItemChanged?.Invoke(item);
            var msg = new EquipmentChanged(owner, holder);
            item.Post(msg);
            owner.Post(msg);
            holder.LastEquipStatus = "";
            return true;
        }

        public bool ClearAndAddToInventory(IEquipmentHolder holder, bool isSwap) {
            var inventory = holder.Owner.Get<ItemInventory>();
            if (inventory == null || inventory.IsFull) {
                holder.LastEquipStatus = "Owner inventory full";
                return false;
            }
            var oldItem = holder.Item;
            ClearEquippedItem(holder, isSwap);
            if (!World.Get<ContainerSystem>().TryAdd(inventory, oldItem)) {
                holder.LastEquipStatus = "Owner inventory rejected old item";
                return false;
            }
            return true;
        }

        public void ClearEquippedItem(IEquipmentHolder holder, bool isSwap) {
            if (holder.Item == null) {
                return;
            }
            var item = holder.Item;
            var owner = holder.Owner;
            var node = item.GetNode<EquipmentNode>();
            ClearStats(holder);
            item.ClearParent(owner);
            holder.Item = null;
            if (node?.Equipment != null) {
                ClearStats(node.Equipment);
            }
            if (node?.Item != null && node.Item.Inventory == holder.Container) {
                node.Item.SetContainer(null);
            }
            item.Post(new EquipmentChanged(null, null));
            if (!isSwap) {
                owner.Post(new EquipmentChanged(owner, holder));
                holder.OnItemChanged?.Invoke(null);
            }
        }

        private void ClearStats(IEquipmentHolder holder) {
            if (holder.CurrentStats == null) {
                return;
            }
            for (int i = 0; i < holder.CurrentStats.Count; i++) {
                holder.CurrentStats[i].Remove();
            }
            holder.CurrentStats.Clear();
        }

        private void ClearStats(Equipment holder) {
            if (holder?.Mods == null) {
                return;
            }
            for (int i = 0; i < holder.Mods.Length; i++) {
                holder.Mods[i].Remove();
            }
            holder.Mods = null;
        }

        public void SetStats(IEquipmentHolder holder, EquipmentNode node) {
            if (node.Equipment == null) {
                return;
            }
            //var ownerStats = SlotOwner.GetEntity().Stats;
            //var skillComponent = Item.Get<SkillRequirement>();
            //var skill = skillComponent != null ? skillComponent.Skill : GameData.Skills.GetID(0);
            //if (TargetSlot == EquipmentSlotType.MainHand) {
            //    bool ranged = Item.Tags.Contain(EntityTags.RangedWeapon);
            //    var charSkill = ownerStats.Get(skill);
            //    if (charSkill != null) {
            //        _currentStats.Add(new DerivedStatModHolder(charSkill, Item.Stats.Get(Stats.ToHit), RpgSettings.SkillStandardToHitBonus));
            //    }
            //    _currentStats.Add(new DerivedStatModHolder(ownerStats.Get(ranged ? Stats.BonusPowerRanged : Stats.BonusPowerMelee), Item.Stats.Get(Stats.Power), 1));
            //    _currentStats.Add(new DerivedStatModHolder(ownerStats.Get(ranged ? Stats.BonusToHitRanged : Stats.BonusToHitMelee), Item.Stats.Get(Stats.ToHit), 1));
            //    _currentStats.Add(new DerivedStatModHolder(ownerStats.Get(ranged ? Stats.BonusCritRanged : Stats.BonusCritMelee), Item.Stats.Get(Stats.CriticalHit), 1));
            //}
            ClearStats(node.Equipment);
            var itemStats = node.Equipment.GetEntity().Get<StatsContainer>();
            if (node.Equipment.Mods == null || node.Equipment.Mods.Length != node.Equipment.StatsToEquip.Count) {
                node.Equipment.Mods = new StatModHolder[node.Equipment.StatsToEquip.Count];
                for (int i = 0; i < node.Equipment.Mods.Length; i++) {
                    node.Equipment.Mods[i] = new DerivedStatModHolder(itemStats.Get(node.Equipment.StatsToEquip[i]), 1);
                }
            }
            var slotStats = holder.Owner.Get<StatsContainer>();
            for (int i = 0; i < node.Equipment.Mods.Length; i++) {
                node.Equipment.Mods[i].Attach(slotStats.Get(node.Equipment.Mods[i].StatID));
            }
        }
        public void HandleGlobal(SaveGameLoaded arg) {
            var equipArray = EntityController.GetComponentArray<Equipment>();
            foreach (Equipment equipment in equipArray) {
                if (equipment.Mods == null) {
                    continue;
                }
                for (int i = 0; i < equipment.Mods.Length; i++) {
                    equipment.Mods[i].Restore();
                }
            }
        }

        public void Handle(DataDescriptionUpdating arg) {
            var equipEntity = arg.Data.GetEntity();
            var equipment = equipEntity.Get<Equipment>();
            if (equipment == null) {
                return;
            }
            FastString.Instance.Clear();
            var stats = equipEntity.Get<StatsContainer>();
            for (int i = 0; i < equipment.StatsToEquip.Count; i++) {
                FastString.Instance.AppendNewLine(stats.Get(equipment.StatsToEquip[i]).ToLabelString());
            }
            FastString.Instance.AppendBoldLabelNewLine("Equipment Slot", GameData.EquipmentSlotTypes.GetNameAt(equipment.EquipmentSlotType));
            arg.Data.Text += FastString.Instance.ToString();
        }

        public void Handle(TooltipDisplaying arg) {
            var equipEntity = arg.Target.GetEntity();
            var equipment = equipEntity.Get<Equipment>();
            if (equipment == null) {
                return;
            }
            var container = equipEntity.Get<InventoryItem>();
            if (container?.Inventory == null) {
                return;
            }
            var equipSlots = container.Inventory.Owner.Get<EquipmentSlots>();
            if (equipSlots == null) {
                return;
            }
            var slot = equipSlots.GetSlot(equipment.EquipmentSlotType);
            if (slot == null) {
                return;
            }
            var compareItem = slot.Item ?? null;
            if (compareItem != null && compareItem.Id != arg.Target.GetEntity()) {
                Game.DisplayCompareData(compareItem);
            }
        }

        

    }

    public class EquipmentNode : BaseNode {

        private CachedComponent<InventoryItem> _item = new CachedComponent<InventoryItem>();
        private CachedComponent<Equipment> _equip = new CachedComponent<Equipment>();
        private CachedComponent<SkillRequirement> _skillRequirement = new CachedComponent<SkillRequirement>();
        
        public Equipment Equipment { get => _equip.Value; }
        public InventoryItem Item { get => _item.Value; }
        public SkillRequirement SkillRequirement { get => _skillRequirement.Value; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _equip, _item, _skillRequirement
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(InventoryItem),
            };
        }
    }
}
