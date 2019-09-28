using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    [AutoRegister]
    public sealed class ConvertVitalSystem : SystemBase, IReceive<ImpactEvent> {

        public ConvertVitalSystem() {
            EntityController.RegisterReceiver(
                new EventReceiverFilter(
                    this, new[] {
                        typeof(ConvertVitalImpact)
                    }));
        }

        public void Handle(ImpactEvent arg) {
            if (arg.Hit <= 0) {
                return;
            }
            var component = arg.Source.Find<ConvertVitalImpact>();
            var sourceEntity = component.GetEntity();
            var stats = sourceEntity.Get<StatsContainer>();
            if (component == null || stats == null) {
                return;
            }
            var originStat = arg.Origin.Stats.GetVital(component.SourceVital);
            var targetStat = arg.Target.Stats.GetVital(component.TargetVital);
            if (originStat == null || targetStat == null) {
                return;
            }
            var power = originStat.Current * component.Percent;
            targetStat.Current -= power;
            originStat.Current += power;
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Append(arg.Origin.GetName());
            logMsg.Append(" Converted ");
            logMsg.Append(power.ToString("F0"));
            logMsg.Append(" ");
            logMsg.Append(targetStat.Label);
            logMsg.Append(" from ");
            logMsg.Append(arg.Target.GetName());
            hoverMsg.Append(RulesSystem.LastQueryString);
            logSystem.PostCurrentStrings(GameLogSystem.DamageColor);
        }
    }
    
    
    [System.Serializable]
    public class ConvertVitalImpact : IComponent {

        public float Percent;
        public string SourceVital;
        public string TargetVital;

        public ConvertVitalImpact(float percent, string source, string target) {
            Percent = percent;
            SourceVital = source;
            TargetVital = target;
        }

        public ConvertVitalImpact(SerializationInfo info, StreamingContext context) {
            Percent = info.GetValue(nameof(Percent), Percent);
            SourceVital = info.GetValue(nameof(SourceVital), SourceVital);
            TargetVital = info.GetValue(nameof(TargetVital), TargetVital);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Percent), Percent);
            info.AddValue(nameof(SourceVital), SourceVital);
            info.AddValue(nameof(TargetVital), TargetVital);
        }
    }
}
