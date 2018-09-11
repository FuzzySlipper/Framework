using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades;

namespace PixelComrades {

    [System.Serializable]
    public abstract class Command : IComponent {
        public abstract Sprite Icon { get; }
        public abstract bool TryComplete();

        [SerializeField] private int _owner;
        public int Owner {
            get { return _owner; }
            set {
                if (_owner == value) {
                    return;
                }
                _owner = value;
                if (_owner < 0) {
                    return;
                }
                EntityOwner = this.GetEntity();
            }
        }
        public Entity EntityOwner { get; private set; }
        public Entity Parent { get; private set; }
        public CommandsContainer Container { get; set; }
        protected Command() {Owner = -1;}
        private List<ICommandCost> _costs = new List<ICommandCost>();
        protected float TimeStart { get; private set; }

        public float TimeActive { get { return TimeManager.TimeUnscaled - TimeStart; } }
        public List<ICommandCost> Costs { get => _costs; }

        public virtual bool CanBeReplacedBy(Command otherCommand) {
            return true;
        }

        public virtual void Cancel() {
            EntityOwner.Tags.Remove(EntityTags.PerformingCommand);
            CommandLog.CommandCompleted(this);
        }

        public virtual bool CanStart() {
            for (int i = 0; i < Costs.Count; i++) {
                if (!Costs[i].CanAct(EntityOwner)) {
                    return false;
                }
            }
            return true;
        }

        public virtual void Complete() {
            EntityOwner.Tags.RemoveWithRoot(EntityTags.PerformingCommand);
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
            Parent = EntityOwner.GetParent();
            TimeStart = TimeManager.TimeUnscaled;
            CommandLog.CommandActive(this);
            EntityOwner.Tags.AddWithRoot(EntityTags.PerformingCommand);
            Post(new StartedCommand(this));
            Post(new ActionStateEvent(EntityOwner, EntityOwner, EntityOwner.GetPosition(), EntityOwner.GetRotation(), ActionStateEvents.Start));
        }

        public void Post<T>(T msg) where T : struct, IEntityMessage {
            EntityOwner.Post<T>(msg);
            if (Parent != null) {
                Parent.Post<T>(msg);
            }
        }

        public void Post(int msg) {
            EntityOwner.Post(msg);
            if (Parent != null) {
                Parent.Post(msg);
            }
        }

        public virtual bool TryStart() {
            var target = EntityOwner.GetSelfOrParent<CommandTarget>();
            if (target != null) {
                var targeting = EntityOwner.GetSelfOrParent<CommandTargeting>();
                if (!targeting.SatisfiesCondition(target)) {
                    return false;
                }
            }
            return World.Get<CommandSystem>().TryAddCommand(this);
        }

        public virtual bool TryStart(Entity target) {
            var cmdTarget = EntityOwner.GetSelfOrParent<CommandTarget>();
            if (cmdTarget != null) {
                cmdTarget.Target = target;
                var targeting = EntityOwner.GetSelfOrParent<CommandTargeting>();
                if (!targeting.SatisfiesCondition(cmdTarget)) {
                    return false;
                }
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
