using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class DeathSystem : SystemBase, IReceive<ImpactEvent>, IReceiveGlobal<RaiseDeadEvent>, IReceiveGlobal<DeathEvent> {

        public DeathSystem() {
            EntityController.RegisterReceiver(
                new EventReceiverFilter(
                    this, new[] {
                        typeof(InstantKillImpact),
                        typeof(RaiseDeadImpact)
                    }));
        }

        public void HandleGlobal(DeathEvent arg) {
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Clear();
            logMsg.Append(arg.Caused.GetName());
            logMsg.Append(" killed ");
            logMsg.Append(arg.Target.GetName());
            logSystem.PostCurrentStrings(GameLogSystem.DeathColor);
            arg.Target.Tags.Add(EntityTags.IsDead);
            arg.Target.Tags.Add(EntityTags.CantMove);
            arg.Target.Tags.Set(EntityTags.CanUnityCollide, 0);

        }

        public void HandleGlobal(RaiseDeadEvent arg) {
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Clear();
            logMsg.Append(arg.Source.GetName());
            logMsg.Append(" resurrected ");
            logMsg.Append(arg.Target.GetName());
            logSystem.PostCurrentStrings(GameLogSystem.DeathColor);
            var entity = arg.Target;
            entity.Tags.Remove(EntityTags.IsDead);
            entity.Tags.Remove(EntityTags.CantMove);
            arg.Target.Tags.Set(EntityTags.CanUnityCollide, 1);
        }
        
        public void Handle(ImpactEvent arg) {
            if (arg.Hit <= 0) {
                return;
            }
            var component = arg.Source.Find<InstantKillImpact>();
            if (component == null) {
                if (arg.Source.Find<RaiseDeadImpact>() != null) {
                    arg.Target.Post(new RaiseDeadEvent(arg.Origin, arg.Target));
                }
                return;
            }
            var success = RulesSystem.DiceRollSuccess(component.Chance);
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Append(arg.Origin.GetName());
            logMsg.Append(" used instant kill on ");
            logMsg.Append(arg.Target.GetName());
            logMsg.Append(success ? " and succeeded" : " and failed");
            hoverMsg.AppendNewLine(RulesSystem.LastQueryString.ToString());
            logSystem.PostCurrentStrings(!success ? GameLogSystem.NormalColor : GameLogSystem.DeathColor);
            if (success) {
                arg.Target.Post(new DeathEvent(arg.Origin, arg.Target, arg, 100));
                arg.Target.Post(new CombatStatusUpdate(arg.Target, "Lethal Hit!", GameLogSystem.DeathColor));
            }
        }
    }
}
