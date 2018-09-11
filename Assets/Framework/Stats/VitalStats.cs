using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    [Priority(Priority.Low)]
    public class VitalStats : ComponentContainer<VitalStat>, IReceive<DamageEvent>, IReceive<HealEvent> {

        public VitalStats() : base(null) {
            for (int i = 0; i < Vitals.Count; i++) {
                Add(new VitalStat(Vitals.GetDescriptionAt(i), Vitals.GetIdAt(i), Vitals.GetAssociatedValue(i)));
            }
        }

        public void Handle(DamageEvent msg) {
            var damage = msg.Amount;
            if (damage <= 0) {
                return;
            }
            this[msg.TargetVital].Current -= msg.Amount;
        }

        public void Handle(HealEvent arg) {
            this[arg.TargetVital].Current += arg.Amount;
        }

        public VitalStat Get(int index) {
            return this[index];
        }

        public VitalStat Get(string id) {
            for (int i = 0; i < List.Count; i++) {
                if (List[i].Id == id) {
                    return List[i];
                }
            }
            return null;
        }

        public void SetMax() {
            for (int i = 0; i < Count; i++) {
                List[i].SetMax();
            }
        }

        public void DoRecovery(float mod) {
            for (int i = 0; i < List.Count; i++) {
                List[i].DoRecover(mod);
            }
        }
    }

}
