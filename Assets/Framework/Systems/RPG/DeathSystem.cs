using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class DeathSystem : SystemBase, IReceiveGlobal<RaiseDeadEvent>, IReceiveGlobal<DeathEvent>, 
        IRuleEventStart<InstantKillEvent>, IRuleEventRun<InstantKillEvent> {

        public DeathSystem() {
            World.Get<RulesSystem>().AddHandler<InstantKillEvent>(this);
        }

        public void HandleGlobal(DeathEvent arg) {
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Clear();
            logMsg.Append(arg.Origin.GetName());
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
            logMsg.Append(arg.Origin.GetName());
            logMsg.Append(" resurrected ");
            logMsg.Append(arg.Target.GetName());
            logSystem.PostCurrentStrings(GameLogSystem.DeathColor);
            var entity = arg.Target;
            entity.Tags.Remove(EntityTags.IsDead);
            entity.Tags.Remove(EntityTags.CantMove);
            arg.Target.Tags.Set(EntityTags.CanUnityCollide, 1);
        }

        public bool CanRuleEventStart(ref InstantKillEvent context) {
            var success = RulesSystem.DiceRollSuccess(context.InstantKill.Chance);
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Append(context.Origin.GetName());
            logMsg.Append(" used instant kill on ");
            logMsg.Append(context.Target.GetName());
            logMsg.Append(success ? " and succeeded" : " and failed");
            hoverMsg.AppendNewLine(RulesSystem.LastQueryString.ToString());
            logSystem.PostCurrentStrings(!success ? GameLogSystem.NormalColor : GameLogSystem.DeathColor);
            return success;
        }

        public void RuleEventRun(ref InstantKillEvent context) {
            context.Target.Post(new DeathEvent(context.Origin, context.Target, context.ImpactEvent, 100));
            context.Target.Post(new CombatStatusUpdate(context.Target, "Lethal Hit!", GameLogSystem.DeathColor));
        }
    }
}
