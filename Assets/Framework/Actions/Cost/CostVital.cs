using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {
    public class CostVital : CommandCost {

        public static GameOptions.CachedFloat SkillPercent = new GameOptions.CachedFloat("SkillVitalCostReductionPerPoint");
        public static GameOptions.CachedFloat SkillMaxReduction = new GameOptions.CachedFloat("SkillVitalCostMaxMultiplier");
        
        public float VitalAmount;
        public string TargetVital;

        private string _skill;

        public CostVital(string targetVital, float vitalAmount, string skill) {
            VitalAmount = vitalAmount;
            TargetVital = targetVital;
            _skill = skill;
        }

        public override void ProcessCost(Entity entity) {
            var vital = entity.FindStat<VitalStat>(TargetVital);
            if (vital == null) {
                return;
                
            }
            var skillMulti = 1f;
            if (!string.IsNullOrEmpty(_skill)) {
                var skillValue = entity.FindStatValue(_skill);
                skillMulti = Mathf.Clamp(1 - (skillValue * SkillPercent.Value), SkillMaxReduction, 1);
            }
            vital.Current -= VitalAmount * skillMulti;
        }

        public override bool CanAct(Entity entity) {
            var stat = entity.FindStat<VitalStat>(GameData.Vitals.GetID(TargetVital));
            if (stat != null && stat.Current >= VitalAmount) {
                return true;
            }
            //entity.Get<StatusUpdateComponent>(e => e.Status = string.Format("Not enough {0}", Vitals.GetDescriptionAt(TargetVital)));
            entity.PostAll(new StatusUpdate("Not enough " + GameData.Vitals.GetNameAt(TargetVital)));
            return false;
        }
    }
}
