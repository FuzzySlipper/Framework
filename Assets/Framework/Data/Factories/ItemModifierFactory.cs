using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class ItemModifierFactory  {
        
        public static void AddModifiers(string modGroup, int level, Entity entity, out DataEntry prefix, out DataEntry suffix) {
            prefix = GetModifier(modGroup, level, true);
            if (prefix != null) {
                SetBonuses(entity, prefix);
            }
             suffix = GetModifier(modGroup, level, false);
            if (suffix != null) {
                SetBonuses(entity, suffix);
            }
        }

        private static Dictionary<string, ShuffleBag<DataEntry>> _prefixBag = new Dictionary<string, ShuffleBag<DataEntry>>();
        private static Dictionary<string, ShuffleBag<DataEntry>> _suffixBag = new Dictionary<string, ShuffleBag<DataEntry>>();
        
        private static void Init() {
            GameData.AddInit(Init);
            _prefixBag.Clear();
            _suffixBag.Clear();
            var modifierList = GameData.GetSheet(DatabaseSheets.ItemModifiers);
            foreach (var loadedDataEntry in modifierList) {
                if (!loadedDataEntry.Value.TryGetValue(DatabaseFields.ModifierGroup, out string modGroup)) {
                    continue;
                }
                var isPrefix = loadedDataEntry.Value.GetValue<bool>(DatabaseFields.IsPrefix);
                var dict = isPrefix ? _prefixBag : _suffixBag;
                if (!dict.TryGetValue(modGroup, out var bag)) {
                    bag = new ShuffleBag<DataEntry>();
                    dict.Add(modGroup, bag);
                }
                bag.Add(loadedDataEntry.Value, (int) loadedDataEntry.Value.GetValue<float>(DatabaseFields.Chance) * 100);
            }
        }

        private static void SetBonuses(Entity entity, DataEntry mod) {
            var bonuses = mod.Get<DataList>(DatabaseFields.Bonuses);
            if (bonuses == null || bonuses.Value.Count == 0) {
                return;
            }
            var equipment = entity.Get<Equipment>();
            for (int i = 0; i < bonuses.Value.Count; i++) {
                var modRow = bonuses.Value[i];
                var statName = modRow.GetValue<string>(DatabaseFields.Stat);
                var bonus = modRow.GetValue<float>(DatabaseFields.Bonus);
                var addToEquip = modRow.GetValue<bool>(DatabaseFields.AddToEquipList);
                if (string.IsNullOrEmpty(statName) || bonus <= 0) {
                    continue;
                }
                if (equipment != null && addToEquip) {
                    equipment.AddStat(statName);
                }
                var stat = entity.Stats.Get(statName);
                if (stat == null) {
                    var baseValue = GameData.Enums.GetFakeEnum(statName).GetAssociatedValue(statName);
                    stat = new BaseStat(statName, statName, baseValue * bonus);
                    entity.Stats.Add(stat);
                }
                else {
                    stat.AddToBase(stat.BaseValue * bonus);
                }
            }
        }


        private static DataEntry GetModifier(string modGroup, int level, bool isPrefix) {
            var baseChance = GameOptions.Get(RpgSettings.PercentBaseRarityChance, 1f);
            var chance = baseChance + (baseChance * GameOptions.Get(RpgSettings.PercentRarityPerLevelMod, 1f) * level);
            if (!Game.DiceRollSuccess(chance)) {
                return null;
            }
            var dict = isPrefix ? _prefixBag : _suffixBag;
            if (dict.Count == 0) {
                Init();
            }
            if (!dict.TryGetValue(modGroup, out var bag)) {
                return null;
            }
            WhileLoopLimiter.ResetInstance();
            while (WhileLoopLimiter.InstanceAdvance()) {
                var mod = bag.Next();
                if (mod == null) {
                    return null;
                }
                if (mod.GetValue<int>(DatabaseFields.MinLevel) <= level) {
                    return mod;
                }
            }
            return null;
        }
    }
}
