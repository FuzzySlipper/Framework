using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    [AutoRegister]
    public sealed class LeachVitalSystem : SystemBase, IReceive<CausedDamageEvent> {

        public LeachVitalSystem() {
            EntityController.RegisterReceiver(
                new EventReceiverFilter(
                    this, new[] {
                        typeof(LeachImpact)
                    }));
        }

        public void Handle(CausedDamageEvent arg) {
            var component = arg.TakeDamage.Impact.Source.Find<LeachImpact>();
            if (component == null) {
                return;
            }
            var originStat = arg.TakeDamage.Origin.Stats.GetVital(component.TargetVital);
            if (originStat == null) {
                return;
            }
            var amt = arg.Amount * component.DamagePercent;
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Append(arg.TakeDamage.Origin.GetName());
            logMsg.Append(" leached ");
            logMsg.Append(amt.ToString("F0"));
            logMsg.Append(" ");
            logMsg.Append(originStat.Label);
            hoverMsg.Append("Original Damage: ");
            hoverMsg.Append(arg.Amount);
            hoverMsg.Append(" * ");
            hoverMsg.Append(component.DamagePercent);
            hoverMsg.Append(" = ");
            hoverMsg.Append(amt.ToString("F0"));
            logSystem.PostCurrentStrings(GameLogSystem.HealColor);
            arg.TakeDamage.Origin.Post(new HealingEvent(amt, arg.TakeDamage.Target, arg.TakeDamage.Origin, component.TargetVital));
        }
    }

    
    
    [System.Serializable]
    public class LeachImpact : IComponent {

        public string TargetVital;
        public float DamagePercent;

        public LeachImpact(string targetVital, float damagePercent) {
            TargetVital = targetVital;
            DamagePercent = damagePercent;
        }
        public LeachImpact(SerializationInfo info, StreamingContext context) {
            TargetVital = info.GetValue(nameof(TargetVital), TargetVital);
            DamagePercent = info.GetValue(nameof(DamagePercent), DamagePercent);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(TargetVital), TargetVital);
            info.AddValue(nameof(DamagePercent), DamagePercent);
        }
    }
}
