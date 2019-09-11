using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Low)]
    public class DamageComponent : ComponentBase, IReceive<DamageEvent>, IReceive<HealEvent> {
        public bool IsDead = false;

        public void Handle(DamageEvent msg) {
            var damage = msg.Amount;
            if (damage <= 0) {
                return;
            }
            float previousValue = 0;
            var vital = Entity.Stats.GetVital(msg.TargetVital);
            if (vital == null) {
                vital = Entity.Stats.GetVital(GameData.Vitals.GetID(msg.TargetVital));
                
            }
            if (vital != null) {
                previousValue = vital.Current;
                vital.Current -= msg.Amount;
            }
            if (msg.Amount > 0) {
                Entity.Post(new CombatStatusUpdate(msg.Amount.ToString("F1"), Color.red));
            }
            if (vital == null || vital != Entity.Stats.HealthStat || vital.Current > 0.05f) {
                return;
            }
            IsDead = true;
            this.GetEntity().Tags.Add(EntityTags.IsDead);
            this.GetEntity().Tags.Add(EntityTags.CantMove);
            this.GetEntity().Post(new DeathEvent(msg.Origin, msg.Target, msg.Amount - previousValue));
        }

        public void Raise() {
            IsDead = false;
            this.GetEntity().Tags.Remove(EntityTags.IsDead);
            this.GetEntity().Tags.Remove(EntityTags.CantMove);
            this.GetEntity().Post(new RaiseDead(this.GetEntity()));
        }

        public void Handle(HealEvent arg) {
            var vital = Entity.Stats.GetVital(arg.TargetVital);
            if (vital == null) {
                vital = Entity.Stats.GetVital(GameData.Vitals.GetID(arg.TargetVital));
            }
            if (vital != null) {
                vital.Current += arg.Amount;
                if (arg.Amount > 0) {
                    Color color = arg.TargetVital == Stats.Health ? Color.green : Color.yellow;
                    Entity.Post(new CombatStatusUpdate(arg.Amount.ToString("F1"), color));
                }
            }
        }
    }
}
