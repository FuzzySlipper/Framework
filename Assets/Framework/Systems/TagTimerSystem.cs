using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public class TagTimerSystem : SystemBase, ISystemUpdate, 
        IReceiveGlobal<ConfusionEvent>, IReceive<ImpactEvent> {

        public TagTimerSystem() {
            EntityController.RegisterReceiver(
                new EventReceiverFilter(
                    this, new[] {
                        typeof(ApplyTagImpact)
                    }));
        }

        private ManagedArray<TagTimerEvent> _timerArray = new ManagedArray<TagTimerEvent>();

        public bool Unscaled { get { return false; } }
        
        public void OnSystemUpdate(float dt) {
            if (_timerArray.UsedCount == 0) {
                return;
            }
            foreach (TagTimerEvent timer in _timerArray) {
                if (timer.TimeEnd > TimeManager.Time) {
                    continue;
                }
                timer.Entity.Tags.Remove(timer.Tag);
                timer.Entity.Post(new TagChangeEvent(timer.Entity, timer.Tag, false));
                switch (timer.Tag) {
                    case EntityTags.IsConfused:
                        if (EntityCount(timer.Entity, timer.Tag) <= 1) {
                            timer.Entity.Post(new ConfusionEvent(timer.Entity, 0, false));
                        }
                        break;
                }
                _timerArray.Remove(timer);
            }
        }

        private int EntityCount(Entity entity, int tag) {
            int cnt = 0;
            foreach (TagTimerEvent timerEvent in _timerArray) {
                if (timerEvent.Entity == entity && timerEvent.Tag == tag) {
                    cnt++;
                }
            }
            return cnt;
        }

        public void HandleGlobal(ConfusionEvent arg) {
            if (!arg.Active) {
                return;
            }
            arg.Entity.Tags.Add(EntityTags.IsConfused);
            if (arg.Length > 0) {
                _timerArray.Add(new TagTimerEvent(arg.Entity, TimeManager.Time + arg.Length, EntityTags.IsConfused));
            }
        }

        public void Handle(ImpactEvent arg) {
            if (arg.Hit <= 0) {
                return;
            }
            var component = arg.Source.Find<ApplyTagImpact>();
            if (component == null) {
                return;
            }
            var success = RulesSystem.DiceRollSuccess(component.Chance);
            var logSystem = World.Get<GameLogSystem>();
            logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
            logMsg.Append(arg.Origin.GetName());
            logMsg.Append(!success ? " failed to apply " : " applied ");
            logMsg.Append(component.Description);
            logMsg.Append(" on ");
            logMsg.Append(arg.Target.GetName());
            hoverMsg.AppendNewLine(RulesSystem.LastQueryString.ToString());
            if (success) {
                hoverMsg.Append(arg.Target.GetName());
                hoverMsg.Append(" has ");
                hoverMsg.Append(component.Description);
                hoverMsg.Append(" for ");
                hoverMsg.Append(component.Length);
                hoverMsg.Append(" ");
                hoverMsg.Append(StringConst.TimeUnits);
            }
            logSystem.PostCurrentStrings(!success ? GameLogSystem.NormalColor : GameLogSystem.DamageColor);
            if (success) {
                arg.Target.Tags.Add(component.Tag);
                if (component.Length > 0) {
                    _timerArray.Add(new TagTimerEvent(arg.Target, TimeManager.Time + component.Length,
                        component.Tag));
                }
                arg.Target.Post(new TagChangeEvent(arg.Target.Entity, component.Tag, true));
            }
        }
    }
}
