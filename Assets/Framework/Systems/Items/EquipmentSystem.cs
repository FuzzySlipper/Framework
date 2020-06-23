using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades;

namespace PixelComrades {
    [AutoRegister]
    public sealed class EquipmentSystem : SystemBase, IReceiveGlobal<SaveGameLoaded>, IReceive<DataDescriptionUpdating>,
        IReceive<TooltipDisplaying> {

        
        public EquipmentSystem() {
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
            var equipNode = item.GetTemplate<EquipmentTemplate>();
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
            var msg = new EquipmentChanged(holder, item);
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
            if (!isSwap && holder.TargetSlot == EquipmentSlotTypes.MainHand) {
                SetCharacterWeaponType(holder.Owner.GetTemplate<CharacterTemplate>(), WeaponTypes.Unarmed);
            }
            return true;
        }

        public void ClearEquippedItem(IEquipmentHolder holder, bool isSwap) {
            if (holder.Item == null) {
                return;
            }
            var item = holder.Item;
            var owner = holder.Owner;
            var node = item.GetTemplate<EquipmentTemplate>();
            ClearStats(holder);
            item.ClearParent(owner);
            holder.Item = null;
            if (node?.Equipment != null) {
                ClearStats(node.Equipment);
            }
            if (node?.Item != null && node.Item.Inventory == holder.Container) {
                node.Item.SetContainer(null);
            }
            item.Post(new EquipmentChanged(null, item));
            if (!isSwap) {
                owner.Post(new EquipmentChanged(holder, null));
                holder.OnItemChanged?.Invoke(null);
            }
            if (!isSwap && holder.TargetSlot == EquipmentSlotTypes.MainHand) {
                SetCharacterWeaponType(holder.Owner.GetTemplate<CharacterTemplate>(), WeaponTypes.Unarmed);
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

        public void SetStats(IEquipmentHolder holder, EquipmentTemplate template) {
            if (template.Equipment == null) {
                return;
            }
            //var ownerStats = SlotOwner.GetEntity().Stats;
            //var skillComponent = Item.Get<SkillRequirement>();
            //var skill = skillComponent != null ? skillComponent.Skill : GameData.Skills.GetID(0);
            ClearStats(template.Equipment);
            var itemStats = template.Stats;
            if (template.Equipment.Mods == null || template.Equipment.Mods.Length != template.Equipment.StatsToEquip.Count) {
                template.Equipment.Mods = new StatModHolder[template.Equipment.StatsToEquip.Count];
                for (int i = 0; i < template.Equipment.Mods.Length; i++) {
                    template.Equipment.Mods[i] = new DerivedStatModHolder(itemStats.Get(template.Equipment.StatsToEquip[i]), 1);
                }
            }
            var character = holder.Owner.GetTemplate<CharacterTemplate>();
            if (holder.TargetSlot == EquipmentSlotTypes.MainHand) {
                var weapon = template.Get<WeaponComponent>();
                if (weapon != null) {
                    SetCharacterWeaponType(character, weapon.WeaponType);
                }
                character.Stats.Get<PassThroughStat>(Stat.AttackAccuracy).SetStat(itemStats.Get(Stat.AttackAccuracy));
                character.Stats.Get<PassThroughStat>(Stat.AttackDamage).SetStat(itemStats.Get(Stat.AttackDamage));
            }
            for (int i = 0; i < template.Equipment.Mods.Length; i++) {
                template.Equipment.Mods[i].Attach(character.Stats.Get(template.Equipment.Mods[i].StatID));
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
            FastString.Instance.AppendBoldLabelNewLine("Equipment Slot", equipment.EquipmentSlotType);
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

        private void SetCharacterWeaponType(CharacterTemplate character, string weaponType) {
            switch (weaponType) {
                case WeaponTypes.Melee:
                    character.GenericData.SetData(GenericDataTypes.WeaponType, WeaponTypes.Melee);
                    character.GenericData.SetData(GenericDataTypes.AttackDamageBonusStat, GenericDataTypes.MeleeDamageStat);
                    character.GenericData.SetData(GenericDataTypes.AttackAccuracyBonusStat, GenericDataTypes.MeleeAccuracyStat);
                    
                    break;
                case WeaponTypes.Ranged:
                    character.GenericData.SetData(GenericDataTypes.WeaponType, WeaponTypes.Ranged);
                    character.GenericData.SetData(GenericDataTypes.AttackDamageBonusStat, GenericDataTypes.RangedDamageStat);
                    character.GenericData.SetData(GenericDataTypes.AttackAccuracyBonusStat, GenericDataTypes.RangedAccuracyStat);
                    
                    break;
                default:
                case WeaponTypes.Unarmed:
                    character.GenericData.SetData(GenericDataTypes.WeaponType, WeaponTypes.Unarmed);
                    character.GenericData.SetData(GenericDataTypes.AttackDamageBonusStat, GenericDataTypes.UnarmedDamageStat);
                    character.GenericData.SetData(GenericDataTypes.AttackAccuracyBonusStat, GenericDataTypes.UnarmedAccuracyStat);
                    character.Stats.Get<PassThroughStat>(Stat.AttackAccuracy).SetStat(character.Stats.Get(Stat.UnarmedAttackAccuracy));
                    character.Stats.Get<PassThroughStat>(Stat.AttackDamage).SetStat(character.Stats.Get(Stat.UnarmedAttackDamage));
                    break;
            }
        }
    }

    public class EquipmentTemplate : BaseTemplate {

        private CachedComponent<InventoryItem> _item = new CachedComponent<InventoryItem>();
        private CachedComponent<Equipment> _equip = new CachedComponent<Equipment>();
        private CachedComponent<SkillRequirement> _skillRequirement = new CachedComponent<SkillRequirement>();
        private CachedComponent<StatsContainer> _stats = new CachedComponent<StatsContainer>();

        public StatsContainer Stats { get => _stats.Value; }
        public Equipment Equipment { get => _equip.Value; }
        public InventoryItem Item { get => _item.Value; }
        public SkillRequirement SkillRequirement { get => _skillRequirement.Value; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _equip, _item, _skillRequirement, _stats
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(InventoryItem),
            };
        }
    }
}
