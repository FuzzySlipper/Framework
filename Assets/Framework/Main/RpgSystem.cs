using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace PixelComrades {
    public partial class RpgSystem : ScriptableSingleton<RpgSystem> {

        [SerializeField] private float _defaultCritMulti = 1.5f;
        [SerializeField] private float _mobXpPerLevel = 10f;
        [SerializeField] private float _specalMobXp = 2.25f;
        [SerializeField] private float _rangeModifier = 1.15f;
        [SerializeField] private float _maxItemPrice = 5000;
        [SerializeField] private float _baseSellPercent = 0.25f;
        [SerializeField] private int _itemRandomLevelRange = 3;
        [SerializeField] private int _maxItemLevel = 10;
        [SerializeField] private float _weightRecoveryPenaltyMulti = 0.05f;
        [SerializeField] private float _defaultAttackMin = 0.2f;
        [SerializeField] private float _defaultAttackMax = 0.2f;
        [SerializeField] private float _defaultCritChance = 0.2f;
        [SerializeField] private float _stealthDistanceModPerCell = 0.2f;
        [SerializeField] private float _unidentifiedSaleModifier = 0.5f;
        [SerializeField] private float _maxStaminaBlockCost = 10;
        [SerializeField] private float _minStaminaBlockCost = 0.5f;
        [SerializeField] private float _staminaBlockPercent = 0.15f;
        [SerializeField] private float _blockShieldBaseChance = 0.15f;
        [SerializeField] private float _shieldBlockingWeightMulti = 4f;
        [SerializeField] private FloatRange _statMultiNpc = new FloatRange(0.75f, 1.25f);
        [SerializeField] private float[] _startingVitals = new float[] { 125, 25, 25 };
        [SerializeField] private float[] _actionDistances = new float[] { 5, 8.5f, 20, 60};
        [SerializeField] private int _maxLevelPower = 99;
        [SerializeField] private int _statDefaults = 5;

        public static float DefaultCritMulti { get { return Main._defaultCritMulti; } set { Main._defaultCritMulti = value; } }
        public static float MobXpPerLevel { get { return Main._mobXpPerLevel; } set { Main._mobXpPerLevel = value; } }
        public static float SpecalMobXp { get { return Main._specalMobXp; } set { Main._specalMobXp = value; } }
        public static float RangeModifier { get { return Main._rangeModifier; } set { Main._rangeModifier = value; } }
        public static float MaxItemPrice { get { return Main._maxItemPrice; } set { Main._maxItemPrice = value; } }
        public static int MaxItemLevel { get { return Main._maxItemLevel; } set { Main._maxItemLevel = value; } }
        public static int MaxLevelPower { get { return Main._maxLevelPower; } set { Main._maxLevelPower = value; } }
        public static float BaseSellPercent { get { return Main._baseSellPercent; } set { Main._baseSellPercent = value; } }
        public static int ItemRandomLevelRange { get { return Main._itemRandomLevelRange; } set { Main._itemRandomLevelRange = value; } }
        public static float WeightRecoveryPenaltyMulti { get { return Main._weightRecoveryPenaltyMulti; } set { Main._weightRecoveryPenaltyMulti = value; } }
        public static float DefaultAttackMin { get { return Main._defaultAttackMin; } set { Main._defaultAttackMin = value; } }
        public static float DefaultAttackMax { get { return Main._defaultAttackMax; } set { Main._defaultAttackMax = value; } }
        public static float DefaultCritChance { get { return Main._defaultCritChance; } set { Main._defaultCritChance = value; } }
        public static float StealthDistanceModPerCell { get { return Main._stealthDistanceModPerCell; } set { Main._stealthDistanceModPerCell = value; } }
        public static float UnidentifiedSaleModifier { get { return Main._unidentifiedSaleModifier; } set { Main._unidentifiedSaleModifier = value; } }
        public static float MaxStaminaBlockCost { get { return Main._maxStaminaBlockCost; } set { Main._maxStaminaBlockCost = value; } }
        public static float MinStaminaBlockCost { get { return Main._minStaminaBlockCost; } set { Main._minStaminaBlockCost = value; } }
        public static float StaminaBlockPercent { get { return Main._staminaBlockPercent; } set { Main._staminaBlockPercent = value; } }
        public static float BlockShieldBaseChance { get { return Main._blockShieldBaseChance; } set { Main._blockShieldBaseChance = value; } }
        public static float ShieldBlockingWeightMulti { get { return Main._shieldBlockingWeightMulti; } set { Main._shieldBlockingWeightMulti = value; } }
        public static float[] StartingVitals { get {  return Main._startingVitals;} }
        public static float[] ActionDistances { get { return Main._actionDistances; } }
        public static FloatRange NpcStatAdjust { get { return Main._statMultiNpc; } }
        public static int StatDefaults { get { return Main._statDefaults; } set { Main._statDefaults = value; } }

        public static float DistanceMulti(Point3 origin, Point3 target) {
            var dist = origin.Distance(target);
            if (dist < 4) {
                return 1;
            }
            if (dist < 15) {
                return 1.5f;
            }
            return Mathf.Clamp(dist  * 0.1f, 1.5f, 8);
        }

        public static float GetDefenseAmount(float damage, float stat) {
            return damage * ((stat / (damage * 10)) * 0.5f);
        }

        public static int ItemLevelCurrentMax {
            get {
                int level = 1;
                for (int i = 0; i < Player.Entities.Length; i++) {
                    level = MathEx.Max(Player.Entities[i].Get<EntityLevelComponent>().Level, level);
                }
                return level;
            }
        }

        public static int ItemRandomCurrentLevel() {
            var max = Mathf.Clamp(ItemLevelCurrentMax, 2, World.Get<MapSystem>().Level);
            return Mathf.Clamp(Game.Random.Next(max - ItemRandomLevelRange, max + ItemRandomLevelRange), 2, 99);
        }

        public static int PriceEstimateSell(Entity item) {
            var inven = item.Get<InventoryItem>();
            if (inven == null) {
                return 100;
            }
            return (int) (inven.Price * (inven.Identified ? 1 : UnidentifiedSaleModifier));
        }

        public static int RepairEstimate(Entity item) {
            //var totalPrice = item.TotalPrice();
            ////return (int) (totalPrice * (1-item.Durability.CurrentPercent));
            //return totalPrice;
            return PriceEstimateSell(item);
        }

        public static float CalculatePrice(float basePrice, int level, int itemCount) {
            return ((int) (basePrice + (basePrice * (level * 0.1f)) + (level * level * level))) * itemCount;
        }

        public static float CalculateValue(float basePower, float percent, int level) {
            return basePower + ((basePower*percent)*level);
        }

        public static int IdentifyEstimate(Entity item) {
            return 100 * item.Get<EntityLevelComponent>().Level;
        }

        //public static bool EvadeDetection(Entity detectingActor, Entity stealthActor, bool isVisible) {
        //    var sneakSkill = stealthActor.Stats.GetHideScore(isVisible);
        //    if (stealthActor.Status.Contains(CharacterStatus.Hidden)) {
        //        sneakSkill *= 2f;
        //    }
        //    var detectSkill = detectingActor.Stats.GetDetection();
        //    var dist = detectingActor.GridPosition.Distance(stealthActor.GridPosition);
        //    detectSkill = MathEx.Max(0, detectSkill - (dist*RpgSystem.StealthDistanceModPerCell));
        //    var success = Game.DiceRollSuccess(sneakSkill - detectSkill);
        //    //if (GameOptions.LogAllCommands) {
        //    //    DebugLogManager.Log(string.Format("{0} tried to avoid {1}: {2}", stealthActor.Name, detectingActor.Name, success), "", LogType.Log);
        //    //}
        //    return success;
        //}
    }
}