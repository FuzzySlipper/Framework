using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {

    public static class StatExtensions {
        private const float Comparison = 0.001f;

        public static void SetupBasicCharacterStats(Entity owner) {
            for (int i = 0; i < GameData.Attributes.Count; i++) {
                owner.Stats.Add(new BaseStat(GameData.Attributes.Names[i], GameData.Attributes.GetID(i), GameData.Attributes.GetAssociatedValue(i)));
            }
            var atkStats = GameData.Enums[Stats.AttackStats];
            if (atkStats != null) {
                for (int i = 0; i < atkStats.Length; i++) {
                    owner.Stats.Add(new BaseStat(atkStats.Names[i], atkStats.IDs[i], 0));
                }
            }
        }

        public static void SetupVitalStats(Entity owner) {
            for (int i = 0; i < GameData.Vitals.Count; i++) {
                owner.Stats.Add(new VitalStat(GameData.Vitals.Names[i], GameData.Vitals.GetID(i), GameData.Vitals.GetAssociatedValue(i)));
            }
        }

        public static void SetupDefenseStats(Entity owner) {
            var defend = owner.Add(new DefendDamageWithStats());
            for (int i = 0; i < GameData.DamageTypes.Count; i++) {
                var typeDef = new BaseStat(string.Format("{0} Defense", GameData.DamageTypes.GetNameAt(i)), GameData.DamageTypes.GetID(i), 0);
                owner.Stats.Add(typeDef);
                defend.AddStat(GameData.DamageTypes.GetID(i), typeDef.ID, typeDef);
            }
            owner.Stats.Add(new BaseStat(Stats.Evasion, 0));
        }

        public static BaseStat[] GetBasicCommandStats() {
            BaseStat[] stats = new BaseStat[3];
            stats[0] = new BaseStat(Stats.Power, 0);
            stats[1] = new BaseStat(Stats.CriticalHit, 0);
            stats[2] = new BaseStat(Stats.CriticalMulti, GameOptions.Get(RpgSettings.DefaultCritMulti, 1f));
            return stats;
        }

        public static void GetCharacterStatValues(this StatsContainer statsContainer, ref StringBuilder sb) {
            for (int i = 0; i < GameData.Attributes.Count; i++) {
                sb.AppendNewLine(statsContainer.Get(GameData.Attributes.GetID(i)).ToString());
            }
            var atkStats = GameData.Enums[Stats.AttackStats];
            if (atkStats != null) {
                for (int i = 0; i < atkStats.Length; i++) {
                    sb.AppendNewLine(statsContainer.Get(atkStats.IDs[i]).ToString());
                }
            }
            for (int i = 0; i < GameData.DamageTypes.Count; i++) {
                sb.AppendNewLine(statsContainer.Get(GameData.DamageTypes.GetID(i)).ToString());
            }
        }

        public static void AddStatList(Entity entity, DataList stats, Equipment equipment) {
            if (stats == null) {
                return;
            }
            for (int i = 0; i < stats.Count; i++) {
                var statEntry = stats[i];
                var statName = statEntry.GetValue<string>(DatabaseFields.Stat);
                if (string.IsNullOrEmpty(statName)) {
                    continue;
                }
                var label = statEntry.TryGetValue("Label", statName);
                var amount = statEntry.GetValue<int>(DatabaseFields.Amount);
                var multiplier = statEntry.GetValue<float>(DatabaseFields.Multiplier);
                var addToEquip = statEntry.GetValue<bool>(DatabaseFields.AddToEquipList);
                if (equipment != null && addToEquip) {
                    equipment.AddStat(statName);
                }
                if (Math.Abs(multiplier) < Comparison || Math.Abs(multiplier - 1) < Comparison) {
                    entity.Stats.GetOrAdd(statName, label).AddToBase(amount);
                    continue;
                }
                var stat = entity.Stats.Get(statName);
                string id = "";
                label = "";
                float adjustedAmount = amount;
                RangeStat rangeStat;
                if (stat != null) {
                    adjustedAmount = stat.BaseValue + amount;
                    rangeStat = stat as RangeStat;
                    if (rangeStat != null) {
                        rangeStat.SetValue(adjustedAmount, adjustedAmount * multiplier);
                        continue;
                    }
                    id = stat.ID;
                    label = stat.Label;
                    entity.Stats.Remove(stat);
                }
                if (string.IsNullOrEmpty(id)) {
                    var fakeEnum = GameData.Enums.GetEnumIndex(statName, out var index);
                    if (fakeEnum != null) {
                        id = fakeEnum.GetID(index);
                        label = fakeEnum.GetNameAt(index);
                    }
                    else {
                        id = label = statName;
                    }
                }
                rangeStat = new RangeStat(label, id, adjustedAmount, adjustedAmount * multiplier);
                entity.Stats.Add(rangeStat);
            }
        }

        public static void AddStatList(Entity entity, DataList stats, float multi) {
            if (stats == null) {
                return;
            }
            for (int i = 0; i < stats.Count; i++) {
                var statEntry = stats[i];
                var statName = statEntry.GetValue<string>(DatabaseFields.Stat);
                if (string.IsNullOrEmpty(statName)) {
                    continue;
                }
                var amount = statEntry.GetValue<int>(DatabaseFields.Amount);
                entity.Stats.GetOrAdd(statName).AddToBase(amount + (amount * multi));
            }
        }

        public static void AddCombatRating(Entity entity) {
            var combatPower = new BaseStat(Stats.CombatRating, "Combat Rating", 0);
            entity.Stats.Add(combatPower);
            var atkStats = GameData.Enums[Stats.AttackStats];
            for (int i = 0; i < atkStats.Length; i++) {
                entity.Stats.Get(atkStats.IDs[i]).AddDerivedStat(1, combatPower);
            }
            for (int i = 0; i < GameData.Attributes.Count; i++) {
                entity.Stats.Get(GameData.Attributes[i]).AddDerivedStat(1, combatPower);
            }
        }
    }
}
