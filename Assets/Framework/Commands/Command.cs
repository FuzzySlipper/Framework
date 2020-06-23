using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
<<<<<<< HEAD
using PixelComrades;
using PixelComrades.DungeonCrawler;
=======
>>>>>>> FirstPersonAction

namespace PixelComrades {

    [System.Serializable]
    public abstract class Command {
        public abstract Sprite Icon { get; }
        public abstract bool TryComplete();

        protected Command(){}

        public TurnBasedCharacterTemplate Owner;

        private List<ICommandCost> _costs = new List<ICommandCost>();
        protected float TimeStart { get; private set; }

        public float TimeActive { get { return TimeManager.TimeUnscaled - TimeStart; } }
        public List<ICommandCost> Costs { get => _costs; }

        public virtual bool CanBeReplacedBy(Command otherCommand) {
            return true;
        }

        public virtual void Cancel() {
<<<<<<< HEAD
=======
            Owner.TurnBased.CurrentCommand = null;
>>>>>>> FirstPersonAction
            Owner.Tags.Remove(EntityTags.PerformingAction);
            CommandLog.CommandCompleted(this);
        }

        public virtual bool CanStart() {
            if (Owner == null) {
                return false;
            }
            for (int i = 0; i < Costs.Count; i++) {
<<<<<<< HEAD
                if (!Costs[i].CanAct(Owner, null)) {
=======
                if (!Costs[i].CanAct(null, Owner)) {
>>>>>>> FirstPersonAction
                    return false;
                }
            }
            return true;
        }

        public virtual void Complete() {
<<<<<<< HEAD
=======
            Owner.TurnBased.CurrentCommand = null;
>>>>>>> FirstPersonAction
            Owner.Tags.RemoveWithRoot(EntityTags.PerformingAction);
            Post(EntitySignals.CommandComplete);
            ProcessCost();
            if (GameOptions.TurnBased) {
                MessageKit.post(EntitySignals.TurnEnded);
            }
            CommandLog.CommandCompleted(this);
        }

        public virtual void ProcessCost() {
            for (int i = 0; i < Costs.Count; i++) {
<<<<<<< HEAD
                Costs[i].ProcessCost(Owner, null);
=======
                Costs[i].ProcessCost(null, Owner);
>>>>>>> FirstPersonAction
            }
        }

        public virtual void StartCommand() {
            Owner.TurnBased.CurrentCommand = this;
            TimeStart = TimeManager.TimeUnscaled;
            CommandLog.CommandActive(this);
            Owner.Tags.AddWithRoot(EntityTags.PerformingAction);
            Post(new StartedCommand(this));
        }

        public virtual void Clear() {
            Owner = null;
        }

        public void Post<T>(T msg) where T : struct, IEntityMessage {
            Owner.Post<T>(msg);
        }

        public void Post(int msg) {
            Owner.Entity.Post(msg);
        }

        public virtual bool TryStart(bool postUpdates = true) {
            if (!CanStart()) {
                return false;
            }
            return World.Get<TurnBasedCommandSystem>().TryAddCommand(this);
        }

        public virtual bool TryStart(CharacterTemplate target, bool postUpdates = true) {
            if (target == null) {
                return TryStart(postUpdates);
            }
            var cmdTarget = Owner.Target;
            if (cmdTarget != null) {
                cmdTarget.Target = target;
            }
            // if (Targeting.Criteria != TargetType.Any && !Targeting.SatisfiesCondition(Owner, target, postUpdates)) {
            //     return false;
            // }
            if (!CanStart()) {
                return false;
            }
            return World.Get<TurnBasedCommandSystem>().TryAddCommand(this);
        }

        protected Command(IList<ICommandCost> costs) {
            if (costs != null) {
                Costs.AddRange(costs);
            }
        }

        public virtual string GetStatus() {
            return Owner != null ? "Running" : "Pooled";
        }
    }

    public struct StartedCommand : IEntityMessage {
        public Command Command;

        public StartedCommand(Command command) {
            Command = command;
        }
    }
}
