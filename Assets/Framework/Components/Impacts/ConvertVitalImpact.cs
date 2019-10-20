using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    [AutoRegister]
    public sealed class ConvertVitalSystem : SystemBase, IRuleEventRun<ConvertVitalEvent> {

        public ConvertVitalSystem() {
            World.Get<RulesSystem>().AddHandler<ConvertVitalEvent>(this);
        }

        public void RuleEventRun(ref ConvertVitalEvent context) {
            var component = context.ConvertVital;
            var originStat = context.Origin.Stats.GetVital(component.SourceVital);
            var targetStat = context.Target.Stats.GetVital(component.TargetVital);
            if (originStat == null || targetStat == null) {
                return;
            }
            var power = originStat.Current * component.Percent;
            targetStat.Current -= power;
            originStat.Current += power;
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Append(context.Origin.GetName());
            logMsg.Append(" Converted ");
            logMsg.Append(power.ToString("F0"));
            logMsg.Append(" ");
            logMsg.Append(targetStat.Label);
            logMsg.Append(" from ");
            logMsg.Append(context.Target.GetName());
            hoverMsg.Append(RulesSystem.LastQueryString);
            logSystem.PostCurrentStrings(GameLogSystem.DamageColor);
        }
    }

    public struct ConvertVitalEvent : IRuleEvent {
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public ActionTemplate Action { get; }
        public ConvertVitalImpact ConvertVital { get; }

        public ConvertVitalEvent(ImpactEvent impactEvent, ConvertVitalImpact convertVital) {
            Action = impactEvent.Action;
            Origin = impactEvent.Origin;
            Target = impactEvent.Target;
            ConvertVital = convertVital;
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
