using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CostVital : CommandCost {

        public float VitalAmount;
        public string TargetVital;

        public CostVital(string targetVital, float vitalAmount) {
            VitalAmount = vitalAmount;
            TargetVital = targetVital;
        }

        public override void ProcessCost(Entity entity) {
            entity.FindStat<VitalStat>(TargetVital, FinishProcess);
        }

        private void FinishProcess(VitalStat v) {
            v.Current -= VitalAmount;
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
