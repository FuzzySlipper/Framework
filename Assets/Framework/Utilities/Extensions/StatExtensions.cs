using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {

    public static class StatExtensions {
        private const float Comparison = 0.001f;

        public static void SetupBasicCharacterStats(StatsContainer stats) {
            var owner = stats.GetEntity();
            for (int i = 0; i < GameData.Attributes.Count; i++) {
                stats.Add(new BaseStat(owner, GameData.Attributes.Names[i], GameData.Attributes.GetID(i), GameData.Attributes.GetAssociatedValue(i)));
            }
            var atkStats = GameData.Enums[Stats.AttackStats];
            if (atkStats != null) {
                for (int i = 0; i < atkStats.Length; i++) {
                    stats.Add(new BaseStat(owner, atkStats.Names[i], atkStats.IDs[i], 0));
                }
            }
        }

        public static void SetupVitalStats(StatsContainer stats) {
            var owner = stats.GetEntity();
            for (int i = 0; i < GameData.Vitals.Count; i++) {
                var vital = new VitalStat(owner, GameData.Vitals.Names[i], GameData.Vitals.GetID(i), GameData.Vitals.GetAssociatedValue(i), GameData.Vitals.GetValue<float>(i, "Recovery"));
                stats.Add(vital);
            }
        }

        public static void SetupDefenseStats(StatsContainer stats) {
            var owner = stats.GetEntity();
            var defend = owner.Add(new DefendDamageWithStats());
            for (int i = 0; i < GameData.DamageTypes.Count; i++) {
                var typeDef = new BaseStat(owner, string.Format("{0} Defense", GameData.DamageTypes.GetNameAt(i)), GameData.DamageTypes.GetID(i), 0);
                stats.Add(typeDef);
                defend.AddStat(GameData.DamageTypes.GetID(i), typeDef.ID, typeDef);
            }
            stats.Add(new BaseStat(owner, Stats.Evasion, 0));
        }

        public static BaseStat[] GetBasicCommandStats(StatsContainer stats) {
            var owner = stats.GetEntity();
            BaseStat[] newStats = new BaseStat[3];
            newStats[0] = new BaseStat(owner, Stats.Power, 0);
            newStats[1] = new BaseStat(owner, Stats.CriticalHit, 0);
            newStats[2] = new BaseStat(owner, Stats.CriticalMulti, GameOptions.Get(RpgSettings.DefaultCritMulti, 1f));
            return newStats;
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

        public static void AddStatList(Entity entity, DataList data, Equipment equipment) {
            if (data == null) {
                return;
            }
            var stats = entity.GetOrAdd<StatsContainer>();
            for (int i = 0; i < data.Count; i++) {
                var statEntry = data[i];
                var statName = statEntry.GetValue<string>(DatabaseFields.Stat);
                if (string.IsNullOrEmpty(statName)) {
                    continue;
                }
                var label = statEntry.TryGetValue("Label", statName);
                var amount = statEntry.GetValue<int>(DatabaseFields.Amount);
                var multiplier = statEntry.GetValue<float>(DatabaseFields.Multiplier);
                var addToEquip = statEntry.GetValue<bool>(DatabaseFields.AddToEquipList);
                if (equipment != null && addToEquip) {
                    if (!equipment.StatsToEquip.Contains(statName)) {
                        equipment.StatsToEquip.Add(statName);
                    }
                }
                if (Math.Abs(multiplier) < Comparison || Math.Abs(multiplier - 1) < Comparison) {
                    stats.GetOrAdd(statName, label).AddToBase(amount);
                    continue;
                }
                var stat = stats.Get(statName);
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
                    stats.Remove(stat);
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
                rangeStat = new RangeStat(entity, label, id, adjustedAmount, adjustedAmount * multiplier);
                stats.Add(rangeStat);
            }
        }

        public static void AddStatList(Entity entity, DataList data, float multi) {
            if (data == null) {
                return;
            }
            var stats = entity.GetOrAdd<StatsContainer>();
            for (int i = 0; i < data.Count; i++) {
                var statEntry = data[i];
                var statName = statEntry.GetValue<string>(DatabaseFields.Stat);
                if (string.IsNullOrEmpty(statName)) {
                    continue;
                }
                var amount = statEntry.GetValue<int>(DatabaseFields.Amount);
                stats.GetOrAdd(statName).AddToBase(amount + (amount * multi));
            }
        }
    }
}
