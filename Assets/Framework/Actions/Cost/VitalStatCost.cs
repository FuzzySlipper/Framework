using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class VitalStatCost : CommandCost {

        private float _amount;
        private VitalStat _stat;

        public VitalStatCost(VitalStat stat, float amount) {
            _amount = amount;
            _stat = stat;
        }

        public override void ProcessCost(Entity entity) {
            _stat.Current -= _amount;
        }

        public override bool CanAct(Entity entity) {
            if (_stat != null && _stat.Current >= _amount) {
                return true;
            }
            //entity.Get<StatusUpdateComponent>(e => e.Status = string.Format("Not enough {0}", Vitals.GetDescriptionAt(TargetVital)));
            entity.PostAll(new StatusUpdate("Not enough " + _stat.Label));
            return false;
        }
    }
}
