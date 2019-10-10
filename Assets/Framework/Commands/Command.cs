using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades;

namespace PixelComrades {

    [System.Serializable]
    public abstract class Command {
        public abstract Sprite Icon { get; }
        public abstract bool TryComplete();

        protected Command(){}

        public Entity EntityOwner;
        public CommandTargeting Targeting;

        private List<ICommandCost> _costs = new List<ICommandCost>();
        protected float TimeStart { get; private set; }

        public float TimeActive { get { return TimeManager.TimeUnscaled - TimeStart; } }
        public List<ICommandCost> Costs { get => _costs; }

        public virtual bool CanBeReplacedBy(Command otherCommand) {
            return true;
        }

        public virtual void Cancel() {
            EntityOwner.Tags.Remove(EntityTags.PerformingAction);
            CommandLog.CommandCompleted(this);
        }

        public virtual bool CanStart() {
            if (EntityOwner == null) {
                return false;
            }
            for (int i = 0; i < Costs.Count; i++) {
                if (!Costs[i].CanAct(EntityOwner)) {
                    return false;
                }
            }
            return true;
        }

        public virtual void Complete() {
            EntityOwner.Tags.RemoveWithRoot(EntityTags.PerformingAction);
            Post(EntitySignals.CommandComplete);
            ProcessCost();
            if (GameOptions.TurnBased) {
                MessageKit.post(EntitySignals.TurnEnded);
            }
            CommandLog.CommandCompleted(this);
        }

        public virtual void ProcessCost() {
            for (int i = 0; i < Costs.Count; i++) {
                Costs[i].ProcessCost(EntityOwner);
            }
        }

        public virtual void StartCommand() {
            TimeStart = TimeManager.TimeUnscaled;
            CommandLog.CommandActive(this);
            EntityOwner.Tags.AddWithRoot(EntityTags.PerformingAction);
            Post(new StartedCommand(this));
            //Post(new ActionStateEvent(EntityOwner, EntityOwner, EntityOwner.GetPosition(), EntityOwner.GetRotation(), ActionStateEvents.Start));
        }

        public void Post<T>(T msg) where T : struct, IEntityMessage {
            EntityOwner.Post<T>(msg);
        }

        public void Post(int msg) {
            EntityOwner.Post(msg);
        }

        public virtual bool TryStart(bool postUpdates = true) {
            if (Targeting.Criteria != TargetType.Any) {
                var target = EntityOwner.GetSelfOrParent<CommandTarget>();
                if (target == null || !Targeting.SatisfiesCondition(EntityOwner, target, postUpdates)) {
                    return false;
                }
            }
            if (!CanStart()) {
                return false;
            }
            return World.Get<CommandSystem>().TryAddCommand(this);
        }

        public virtual bool TryStart(Entity target, bool postUpdates = true) {
            if (target == null) {
                return TryStart(postUpdates);
            }
            var cmdTarget = EntityOwner.GetSelfOrParent<CommandTarget>();
            if (cmdTarget != null) {
                cmdTarget.Target = target;
            }
            if (Targeting.Criteria != TargetType.Any && !Targeting.SatisfiesCondition(EntityOwner, target, postUpdates)) {
                return false;
            }
            if (!CanStart()) {
                return false;
            }
            return World.Get<CommandSystem>().TryAddCommand(this);
        }

        protected Command(IList<ICommandCost> costs) {
            if (costs != null) {
                Costs.AddRange(costs);
            }
        }
    }

    public struct StartedCommand : IEntityMessage {
        public Command Command;

        public StartedCommand(Command command) {
            Command = command;
        }
    }
}
