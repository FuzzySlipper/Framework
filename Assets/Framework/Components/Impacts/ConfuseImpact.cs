using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    [AutoRegister]
    public sealed class ConfuseSystem : SystemBase, IReceive<ImpactEvent> {
        
        private static GameOptions.CachedBool _allowPlayerConfusion = new GameOptions.CachedBool("AllowPlayerConfusion");
        
        public ConfuseSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(ConfuseImpact)
            }));
        }

        public void Handle(ImpactEvent arg) {
            if (arg.Hit <= 0 || (!_allowPlayerConfusion && arg.Target.IsPlayer())) {
                return;
            }
            var component = arg.Source.Find<ConfuseImpact>();
            if (component == null) {
                return;
            }
            var success = RulesSystem.DiceRollSuccess(component.Chance);
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Append(arg.Origin.GetName());
            logMsg.Append(!success ? " failed to confuse " : " confused ");
            logMsg.Append(arg.Target.GetName());
            hoverMsg.AppendNewLine(RulesSystem.LastQueryString.ToString());
            if (success) {
                hoverMsg.Append(arg.Target.GetName());
                hoverMsg.Append(" confused for ");
                hoverMsg.Append(component.Length);
                hoverMsg.Append(" ");
                hoverMsg.Append(StringConst.TimeUnits);
            }
            logSystem.PostCurrentStrings(!success ? GameLogSystem.NormalColor : GameLogSystem.DamageColor);
            if (success) {
                arg.Target.Post(new ConfusionEvent(arg.Target.Entity, component.Length, true));
            }
        }
    }
    
    
    [System.Serializable]
    public sealed class ConfuseImpact : IComponent {

        public float Chance;
        public float Length;


        public ConfuseImpact(float chance, float length) {
            Chance = chance;
            Length = length;
        }

        public void ProcessImpact(CollisionEvent collisionEvent, ActionStateEvent stateEvent) {
            
        }

        public ConfuseImpact(SerializationInfo info, StreamingContext context) {
            Length = info.GetValue(nameof(Length), Length);
            Chance = info.GetValue(nameof(Chance), Chance);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Length), Length);
            info.AddValue(nameof(Chance), Chance);
        }
    }
}
