using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class AmmoSystem : SystemBase, IRuleEventStart<ImpactEvent> {

        private static GameOptions.CachedFloat _skillPercent = new GameOptions.CachedFloat("SkillAmmoReductionPerPoint");
        private static GameOptions.CachedFloat _skillMaxReduction = new GameOptions.CachedFloat("SkillAmmoMaxMultiplier");
        
        public AmmoSystem(){}

        public bool CanLoadAmmo(AmmoComponent ammo) {
            if (ammo.Amount.Value == ammo.Amount.MaxValue) {
                return false;
            }
            for (int i = 0; i < ammo.Config.Cost.Count; i++) {
                if (Player.GetCurrency(ammo.Config.Cost[i].Key).Value < ammo.Config.Cost[i].Value) {
                    return false;
                }
            }
            return true;
        }

        public bool TryLoadOneAmmo(Entity context, AmmoComponent ammo) {
            if (!CanLoadAmmo(ammo)) {
                return false;
            }
            var skillMulti = 1f;
            if (!string.IsNullOrEmpty(ammo.Skill) && context.Get<StatsContainer>().GetValue(ammo.Skill, out var skillValue)) {
                skillMulti = Mathf.Clamp(1 - (skillValue * _skillPercent.Value), _skillMaxReduction, 1);
            }
            for (int i = 0; i < ammo.Config.Cost.Count; i++) {
                Player.GetCurrency(ammo.Config.Cost[i].Key).ReduceValue(ammo.Config.Cost[i].Value * skillMulti);
            }
            ammo.Amount.AddToValue(1);
            return true;
        }

        public bool CanRuleEventStart(ref ImpactEvent context) {
            var ammo = context.Action?.Get<AmmoComponent>();
            if (ammo?.DamageModStat == null) {
                return true;
            }
            ammo.Amount.ReduceValue(1);
            if (ammo.Amount > 0) {
                if (!string.IsNullOrEmpty(ammo.DamageModId)) {
                    ammo.DamageModStat.Stat.RemoveMod(ammo.DamageModId);
                }
                return true;
            }
            if (!string.IsNullOrEmpty(ammo.DamageModId)) {
                ammo.DamageModId = ammo.DamageModStat.Stat.AddValueMod(-(ammo.DamageModStat.Stat.BaseValue * ammo.DamagePercent));
            }
            return true;
        }
    }
}
