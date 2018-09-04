using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface ICommandElement {
        CommandSequence Owner { get; set; }
        ActionStateEvents StateEvent { get; }
        void Start(Entity entity);
    }

    public class CommandSequence : Command {

        public CommandSequence(IList<ICommandElement> elements) {
            if (elements != null) {
                for (int i = 0; i < elements.Count; i++) {
                    elements[i].Owner = this;
                }
                _list.AddRange(elements);
            }
        }

        public CommandSequence(IList<ICommandElement> elements, IList<ICommandCost> costs) : base(costs) {
            if (elements != null) {
                for (int i = 0; i < elements.Count; i++) {
                    elements[i].Owner = this;
                }
                _list.AddRange(elements);
            }
        }

        public CommandTarget Target { get; private set; }
        public int CurrentData = -1;
        public ActionStateEvent? LastStateEvent;
        public override Sprite Icon { get { return EntityOwner.Get<IconComponent>()?.Sprite; } }

        
        private int _current;
        private List<ICommandElement> _list = new List<ICommandElement>();
        private ICommandElement Current => _current < _list.Count ? _list[_current] : null;
        
        public override bool CanStart() {
            return true;
        }

        public override bool TryComplete() {
            return Current == null;
        }

        public void Add(ICommandElement commandElement) {
            commandElement.Owner = this;
            _list.Add(commandElement);
        }

        public override void StartCommand() {
            base.StartCommand();
            _current = 0;
            LastStateEvent = null;
            CurrentData = -1;
            Target = EntityOwner.Get<CommandTarget>();
            if (Current != null) {
                Current.Start(EntityOwner);
            }
        }

        public void Advance() {
            _current++;            
            if (Current != null) {
                Current.Start(EntityOwner);
            }
            else {
                EntityOwner.Post(new CommandComplete(EntityOwner));
            }
        }

        public void DefaultPostAdvance(ICommandElement element) {
            if (element.StateEvent != ActionStateEvents.None) {
                LastStateEvent = new ActionStateEvent(EntityOwner, Target?.Target.Id ?? -1, EntityOwner.GetPosition(), EntityOwner.GetRotation(), element.StateEvent);
                LastStateEvent.Value.Post(EntityOwner);
            }
            Advance();
        }

        public void Post(ActionStateEvent stateEvent) {
            LastStateEvent = stateEvent;
            LastStateEvent.Value.Post(this.GetEntity());
        }
    }

    public struct CommandComplete : IEntityMessage {
        public Entity Owner;
        public CommandComplete(Entity owner) {
            Owner = owner;
        }
    }
}
