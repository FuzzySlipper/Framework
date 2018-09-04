using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CostVital : CommandCost {

        public float VitalAmount;
        public int TargetVital;

        private bool _checkParent;

        public CostVital(bool checkParent, int targetVital, float vitalAmount) {
            VitalAmount = vitalAmount;
            TargetVital = targetVital;
            _checkParent = checkParent;
        }

        public override void ProcessCost(Entity entity) {
            var targetEntity = _checkParent ? entity.GetParent() : entity;
            targetEntity?.Get<VitalStats>(v => v[TargetVital].Current -= VitalAmount);
        }

        public override bool CanAct(Entity entity) {
            var stats = _checkParent ? entity.GetParent()?.Get<VitalStats>() : entity.Get<VitalStats>();
            if (stats == null) {
                return true;
            }
            if (stats[TargetVital].Current >= VitalAmount) {
                return true;
            }
            entity.Get<StatusUpdateComponent>(e => e.Status = string.Format("Not enough {0}", Vitals.GetDescriptionAt(TargetVital)));
            return false;
        }
    }
}
