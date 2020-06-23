using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace PixelComrades {

    public static partial class RpgSettings {
        public static GameOptions.CachedInt MaxStartSkills = new GameOptions.CachedInt("MaxStartSkills");
        public static GameOptions.CachedInt StartingPlayerAttributes = new GameOptions.CachedInt("StartingPlayerAttributes");
        public static GameOptions.CachedInt MinLockpickSkill = new GameOptions.CachedInt("MinLockpickSkill");
        public static GameOptions.CachedString LockpickSkill = new GameOptions.CachedString("LockpickSkill");
        public static GameOptions.CachedString ScoutingSkill = new GameOptions.CachedString("ScoutingSkill");
        
        public static GameOptions.Cached<IntRange> NpcGoldRewardRange =
            new GameOptions.Cached<IntRange>("NpcGoldRewardRange", IntRange.Parse);

        public static GameOptions.CachedFloat[] RankTrainingScaling;
        public static GameOptions.CachedInt[] RankMaxStats;


        public const int MaxPlayerSupplies = 12;
        public const int TurnsToUseSupply = 900;
        public const int IncreaseClassStartLevelCost = 1000;
        public const int IncreaseClassXpMultiCost = 3000;
        public const float IncreaseClassXpMultiAmount = 0.2f;
        public const float PickedSkillStart = 8;
        public const float KnowledgeSkillBonus = 0.05f;
        public const float SkillToHitBonus = 1;
        public const int MaxSkillRank = 4;
        public static string HealthStat = "HealthStat";
        public static string DefaultCritMulti = "DefaultCritMulti";
        public static string ItemRandomLevelRange = "ItemRandomLevelRange";
        public static string UnidentifiedSaleModifier = "UnidentifiedSaleModifier";
        public static string PercentBaseRarityChance = "PercentBaseRarityChance";
        public static string PercentRarityPerLevelMod = "PercentRarityPerLevelMod";
        

        public static int[] SpellLevels = new[] {
            1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 6, 6, 6, 7, 7, 8
        };

        public static float GetTrainingScaling(int maxRank) {
            if (RankTrainingScaling == null) {
                RankTrainingScaling = new GameOptions.CachedFloat[MaxSkillRank];
                for (int i = 0; i < RankTrainingScaling.Length; i++) {
                    RankTrainingScaling[i] = new GameOptions.CachedFloat("RankTrainingScale" + i);
                }
            }
            return RankTrainingScaling.SafeAccess((int) maxRank).Value;
        }

        public static float GetMaxStat(int maxRank) {
            if (RankMaxStats == null) {
                RankMaxStats = new GameOptions.CachedInt[MaxSkillRank];
                for (int i = 0; i < RankMaxStats.Length; i++) {
                    RankMaxStats[i] = new GameOptions.CachedInt("RankMaxStat" + i);
                }
            }
            return RankMaxStats.SafeAccess((int) maxRank).Value;
        }

        public static bool CanOpenLock(Entity entity, float difficulty) {
            var skill = entity.Get<StatsContainer>().Get<SkillStat>(LockpickSkill);
            if (skill == null || skill.CurrentRank < MinLockpickSkill.Value) {
                return false;
            }
            var total = skill.Value - difficulty;
            if (Game.DiceRollSuccess(total)) {
                return true;
            }
            return false;
        }


    }
}