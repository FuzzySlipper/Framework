using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Serializable]
    public class VitalStatCost : CommandCost, ISerializable {

        private float _amount;
        private CachedStat<VitalStat> _stat;

        public VitalStatCost(VitalStat stat, float amount) {
            _amount = amount;
            _stat = new CachedStat<VitalStat>(stat);
        }

        public VitalStatCost(SerializationInfo info, StreamingContext context) {
            _amount = info.GetValue(nameof(_amount), _amount);
            _stat = info.GetValue(nameof(_stat), _stat);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_amount), _amount);
            info.AddValue(nameof(_stat), _stat);
        }

        public override void ProcessCost(Entity entity) {
            _stat.Stat.Current -= _amount;
        }

        public override bool CanAct(Entity entity) {
            if (_stat != null && _stat.Stat.Current >= _amount) {
                return true;
            }
            //entity.Get<StatusUpdateComponent>(e => e.Status = string.Format("Not enough {0}", Vitals.GetDescriptionAt(TargetVital)));
            entity.PostAll(new StatusUpdate("Not enough " + _stat.Stat.Label));
            return false;
        }
    }
}
