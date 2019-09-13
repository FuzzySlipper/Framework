using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Priority(Priority.Low)]
    [System.Serializable]
	public sealed class DamageComponent : IComponent, IReceive<DamageEvent>, IReceive<HealEvent> {

        public DamageComponent(){}
        public DamageComponent(SerializationInfo info, StreamingContext context) {}
        public void GetObjectData(SerializationInfo info, StreamingContext context) {}
        
        public void Handle(DamageEvent msg) {
            var damage = msg.Amount;
            if (damage <= 0) {
                return;
            }
            var entity = this.GetEntity();
            float previousValue = 0;
            var vital = entity.Stats.GetVital(msg.TargetVital);
            if (vital == null) {
                vital = entity.Stats.GetVital(GameData.Vitals.GetID(msg.TargetVital));
                
            }
            if (vital != null) {
                previousValue = vital.Current;
                vital.Current -= msg.Amount;
            }
            if (msg.Amount > 0) {
                entity.Post(new CombatStatusUpdate(msg.Amount.ToString("F1"), Color.red));
            }
            if (vital == null || vital != entity.Stats.HealthStat || vital.Current > 0.05f) {
                return;
            }
            entity.Tags.Add(EntityTags.IsDead);
            entity.Tags.Add(EntityTags.CantMove);
            entity.Post(new DeathEvent(msg.Origin, msg.Target, msg.Amount - previousValue));
        }

        public void Raise() {
            var entity = this.GetEntity();
            entity.Tags.Remove(EntityTags.IsDead);
            entity.Tags.Remove(EntityTags.CantMove);
            entity.Post(new RaiseDead(this.GetEntity()));
        }

        public void Handle(HealEvent arg) {
            var entity = this.GetEntity();
            var vital = entity.Stats.GetVital(arg.TargetVital);
            if (vital == null) {
                vital = entity.Stats.GetVital(GameData.Vitals.GetID(arg.TargetVital));
            }
            if (vital != null) {
                vital.Current += arg.Amount;
                if (arg.Amount > 0) {
                    Color color = arg.TargetVital == Stats.Health ? Color.green : Color.yellow;
                    entity.Post(new CombatStatusUpdate(arg.Amount.ToString("F1"), color));
                }
            }
        }
    }
}
