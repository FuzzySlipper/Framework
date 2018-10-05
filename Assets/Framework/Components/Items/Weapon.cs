using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    public class Weapon : IComponent, IReceive<EquipmentChanged> {
        private int _owner = -1;
        public int Owner {
            get { return _owner; }
            set {_owner = value; }
        }

        private CommandSequence _attack;
        private CommandsContainer _current;

        public CommandSequence Attack { get => _attack; }

        public Weapon(CommandSequence attack) {
            _attack = attack;
        }

        public void Handle(EquipmentChanged arg) {
            if (_current != null) {
                _current.Remove(_attack);
                _current.GetEntity().Get<DefaultCommand>(d => d.Alternative = null);
                _current = null;
            }
            if (arg.Owner != null) {
                _current = arg.Owner.Get<CommandsContainer>();
            }
            if (_current != null) {
                _current.Add(_attack);
                _current.GetEntity().Get<DefaultCommand>(d => d.Alternative = _attack);
            }
        }
    }
}
