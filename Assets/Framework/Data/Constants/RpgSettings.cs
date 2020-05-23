using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace PixelComrades {

    public class RpgSettings : SimpleScriptableDatabase<RpgSettings> {
        
        public static GameOptions.CachedInt MaxStartSkills = new GameOptions.CachedInt("MaxStartSkills");
        public static GameOptions.CachedInt StartingPlayerAttributes = new GameOptions.CachedInt("StartingPlayerAttributes");
        public static GameOptions.CachedInt MinLockpickSkill = new GameOptions.CachedInt("MinLockpickSkill");
        public static GameOptions.CachedString LockpickSkill = new GameOptions.CachedString("LockpickSkill");
        public static GameOptions.CachedString ScoutingSkill = new GameOptions.CachedString("ScoutingSkill");
        public static GameOptions.Cached<IntRange> NpcGoldRewardRange = new GameOptions.Cached<IntRange>("NpcGoldRewardRange", IntRange.Parse);
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
        
        public static bool CanSee(Entity entity, MinimapObjectType objectType) {
            var skill = entity.Get<StatsContainer>().Get<SkillStat>(ScoutingSkill);
            if (skill == null) {
                return false;
            }
            if (skill.Value < 20) {
                return false;
            }
            if (skill.Value > 80) {
                return true;
            }
            switch (objectType) {
                default:
                case MinimapObjectType.Trap:
                case MinimapObjectType.Chest:
                case MinimapObjectType.Misc:
                    if (skill.Value > 40) {
                        return true;
                    }
                    break;
                case MinimapObjectType.HiddenDoor:
                case MinimapObjectType.Switch:
                    if (skill.Value > 80) {
                        return true;
                    }
                    break;
            }
            return false;
        }

        public static float PhilologyScore(Entity owner) {
            if (owner == null) {
                return 0;
            }
            return owner.Get<StatsContainer>().GetValue("Scribing");
        }

        //private static float SkillScore(PlayerCharacterNode owner, int skill) {
        //    float mod = 1;
        //    switch (skill) {
        //        case Skills.Philology:
        //            mod = PhilologySkill.GetIdMod(owner.SkillStats.c[Skills.Philology].CurrentRank);
        //            break;
        //        case Skills.Science:
        //            mod = ScienceSkill.GetIdMod(owner.SkillStats.c[Skills.Science].CurrentRank);
        //            break;
        //    }
        //    var skillStat = owner.SkillStats.c[skill];
        //    var skillAmt = skillStat.Value;
        //    return skillAmt * mod;
        //}

        //private static bool PassItemCheck(InventoryItem item, float skillAmt) {
        //    var neededSkill = ((item.Price / MaxItemPrice) * 100) - 1;
        //    if (skillAmt < neededSkill) {
        //        return false;
        //    }
        //    item.Identified = true;
        //    return true;
        //}

        //public static float CurrentLevelNormalized() {
        //    return Mathf.InverseLerp(GlobalLevelController.LevelPowerLevel, 1, MaxLevelPower);
        //}


        //public static int SalePrice(InventoryItem item) {
        //    var sellMod = BaseSellPercent;
        //    return (int) (item.Price * sellMod);
        //}

        public static int CalculateSpellPoints(float willPower, int spellLevel, float spellSkill) {
            var maxSpellPoints = (10 + (willPower / 2)) - (spellLevel - 1);
            float min = (spellLevel * spellLevel) + (3f * spellLevel);
            float max = 50 + (spellLevel * spellLevel) + (5f * spellLevel);
            var spellPoints = (maxSpellPoints * Mathf.Lerp(spellSkill, min, max));
            spellPoints = Mathf.Clamp(spellPoints, 0, maxSpellPoints);
            return (int) spellPoints;
        }

    }
}