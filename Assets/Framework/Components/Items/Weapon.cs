using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    public class Weapon : IComponent, IReceive<EquipmentChanged> {
        private int _owner;
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
                Handle(this.GetEntity());
            }
        }

        private WeaponTemplate _template;
        private int _level;
        private CommandSequence _attack;
        private CommandsContainer _current;

        public WeaponTemplate Template { get { return _template; } }
        public ItemAttackModifier PrefixAttack { get; }
        public ItemAttackModifier SuffixAttack { get; }
        public ActorAnimations Animation { get { return ActorAnimations.Action; } }
        public TargetType Targeting { get { return TargetType.Enemy; } }
        public CommandSequence Attack => _attack;

        public Weapon(WeaponTemplate template, int level, ItemModifier prefix, ItemModifier suffix, CommandSequence attack) {
            _template = template;
            PrefixAttack = prefix as ItemAttackModifier;
            SuffixAttack = suffix as ItemAttackModifier;
            _level = level;
            _attack = attack;
        }

        public void Handle(Entity entity) {
            StringBuilder sb = new StringBuilder();
            if (PrefixAttack != null) {
                PrefixAttack.Init(_level, entity);
                sb.Append(PrefixAttack.DescriptiveName);
                sb.Append(" ");
            }
            sb.Append(_template.Name);
            if (SuffixAttack != null) {
                SuffixAttack.Init(_level, entity);
                sb.Append(" ");
                sb.Append(SuffixAttack.DescriptiveName);
            }
            entity.Add(new LabelComponent(sb.ToString()));
        }

        //protected override bool CanStart() {
        //    if (_weapon.Jammed) {
        //        LastStatusUpdate = "Weapon Jammed";
        //        return false;
        //    }
        //    if (Owner != null && Owner.VitalStats[Vitals.Energy].Current <= 0) {
        //        LastStatusUpdate = "Too tired";
        //        return false;
        //    }
        //    return base.CanStart();
        //}


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
