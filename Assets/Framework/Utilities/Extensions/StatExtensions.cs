using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {

    public static class StatExtensions {
        public static List<BaseStat> GatherCharacterStats() {
            List<BaseStat> basicStats = new List<BaseStat>();
            for (int i = 0; i < Attributes.Count; i++) {
                basicStats.Add(new BaseStat(Attributes.GetDescriptionAt(i), Attributes.GetIdAt(i), RpgSystem.StatDefaults));
            }
            for (int i = 0; i < AttackStats.AllStats.Length; i++) {
                basicStats.Add(new BaseStat(AttackStats.AllStats[i], 0));
            }
            return basicStats;
        }

        public static void SetupDefenseStats(Entity owner) {
            var defend = owner.Add(new DefendDamageWithStats());
            var genericStats = owner.Get<GenericStats>();
            for (int i = 0; i < DamageTypes.Count; i++) {
                var typeDef = new BaseStat(string.Format("{0} Defense", DamageTypes.GetDescriptionAt(i)), DamageTypes.GetIdAt(i), 0);
                genericStats.Add(typeDef);
                defend.AddStat(i, typeDef.Id, typeDef);
            }
            genericStats.Add(new BaseStat(Stats.Evasion, 0));
        }

        public static BaseStat[] GetBasicCommandStats() {
            BaseStat[] stats = new BaseStat[4];
            stats[0] = new BaseStat(Stats.Power, 0);
            stats[1] = new BaseStat(Stats.ToHit, 0);
            stats[2] = new BaseStat(Stats.CriticalHit, 0);
            stats[3] = new BaseStat(Stats.CriticalMulti, RpgSystem.DefaultCritMulti);
            return stats;
        }

        public static void GetCharacterStatValues(this GenericStats stats, ref StringBuilder sb) {
            for (int i = 0; i < Attributes.Count; i++) {
                sb.AppendNewLine(stats.Get(Attributes.GetIdAt(i)).ToString());
            }
            for (int i = 0; i < AttackStats.AllStats.Length; i++) {
                sb.AppendNewLine(stats.Get(AttackStats.AllStats[i]).ToString());
            }
            for (int i = 0; i < DamageTypes.Count; i++) {
                sb.AppendNewLine(stats.Get(DamageTypes.GetIdAt(i)).ToString());
            }
        }
    }

}
