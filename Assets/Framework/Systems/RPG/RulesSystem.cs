using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class RulesSystem : SystemBase {
        
        public static readonly FastString LastQueryString = new FastString();
        
        public static bool DiceRollSuccess(float chance) {
            LastQueryString.Clear();
            LastQueryString.Append("Rolled D100 against ");
            if (chance < 1 && chance > 0) {
                chance *= 100;
            }
            LastQueryString.Append(chance.ToString("F0"));
            LastQueryString.Append("% Chance. Result: ");
            var roll = Game.Random.Next(0, 101);
            LastQueryString.AppendBold(roll.ToString("F0"));
            bool success = roll <= chance;
            LastQueryString.Append(success ? " Success!" : " Failure!");
            return success;
        }

        public static float CalculateTotal(StatsContainer stats, string statName, float percent) {
            var range = stats.Get<RangeStat>(statName);
            if (range != null) {
                return CalculateTotal(range, percent);
            }
            var stat = stats.Get<BaseStat>(statName);
            if (stat != null) {
                return CalculateTotal(stat, percent);
            }
            return 0;
        }

        public static float CalculateTotal(BaseStat stat, float percent) {
            LastQueryString.Clear();
            LastQueryString.Append(stat.Label);
            LastQueryString.Append(": ");
            LastQueryString.Append(stat.BaseValue.ToString("F0"));
            LastQueryString.Append(" + ");
            LastQueryString.AppendNewLine(stat.ModTotal.ToString("F0"));
            LastQueryString.Append(" = ");
            var result = stat.BaseValue + stat.ModTotal;
            LastQueryString.Append(result.ToString("F0"));
            if (Math.Abs(percent - 1) > 0.0001f) {
                LastQueryString.Append(" * ");
                LastQueryString.Append(percent.ToString("F1"));
                LastQueryString.Append(" Final: ");
                result *= percent;
                LastQueryString.Append(result.ToString("F0"));
            }
            return result;
        }
        
        public static float CalculateTotal(RangeStat stat, float percent) {
            LastQueryString.Clear();
            LastQueryString.Append("Base ");
            LastQueryString.Append(stat.Label);
            LastQueryString.Append(": ");
            LastQueryString.Append(stat.BaseValue.ToString("F0"));
            LastQueryString.Append("-");
            LastQueryString.Append((stat.BaseValue + stat.MaxModifier).ToString("F0"));
            LastQueryString.Append(" + ");
            LastQueryString.AppendNewLine(stat.ModTotal.ToString("F0"));
            var roll = Game.Random.NextFloat(stat.BaseValue, stat.BaseValue + stat.MaxModifier);
            LastQueryString.Append("Rolled ");
            LastQueryString.Append(roll.ToString("F0"));
            LastQueryString.Append(" + ");
            LastQueryString.Append(stat.ModTotal.ToString("F0"));
            LastQueryString.Append(" = ");
            var result = roll + stat.ModTotal;
            LastQueryString.Append(result.ToString("F0"));
            if (Math.Abs(percent - 1) > 0.0001f) {
                LastQueryString.Append(" * ");
                LastQueryString.Append(percent.ToString("F1"));
                LastQueryString.Append(" Final: ");
                result *= percent;
                LastQueryString.Append(result.ToString("F0"));
            }
            return result;
        }
        
        public static float GetDefenseAmount(float damage, float stat) {
            return damage * ((stat / (damage * 10)) * 0.5f);
        }

        public static int TotalPrice(InventoryItem item) {
            return item.Price * item.Count;
        }
    }
}
