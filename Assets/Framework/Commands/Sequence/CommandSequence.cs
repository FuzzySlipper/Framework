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
        
        public override bool CanBeReplacedBy(Command otherCommand) {
            return Current == null;
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
            Target = EntityOwner.Find<CommandTarget>();
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
                Post(new CommandComplete(EntityOwner));
            }
        }

        public void DefaultPostAdvance(ICommandElement element) {
            if (element.StateEvent != ActionStateEvents.None) {
                Post(Target?.Target ?? EntityOwner, EntityOwner.GetPosition(), EntityOwner.GetRotation(), element.StateEvent);
            }
            Advance();
        }

        public void PostAdvance(Entity target, Vector3 position, Quaternion rotation, ActionStateEvents state) {
            Post(target, position, rotation, state);
            Advance();
        }

        public void Post(Entity target, Vector3 position, Quaternion rotation, ActionStateEvents state) {
            if (state == ActionStateEvents.None) {
                return;
            }
            LastStateEvent = new ActionStateEvent(EntityOwner, target, position, rotation, state);
            Post(LastStateEvent.Value);
        }
    }

    public struct CommandComplete : IEntityMessage {
        public Entity Owner;
        public CommandComplete(Entity owner) {
            Owner = owner;
        }
    }
}
