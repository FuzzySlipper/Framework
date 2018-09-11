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
        private bool _jammed = false;
        private int _level;
        private CommandSequence _attack;
        private CommandsContainer _current;

        public bool IsRangedWeapon { get { return _template.ItemType != ItemTypes.WeaponMelee; } }
        public bool Jammed { get { return _jammed; } }
        public WeaponTemplate Template { get { return _template; } }
        public bool TwoHanded { get { return _template.TwoHanded; } }
        public ItemAttackModifier PrefixAttack { get; }
        public ItemAttackModifier SuffixAttack { get; }
        public ActorAnimations Animation { get { return ActorAnimations.Action; } }
        public TargetType Targeting { get { return TargetType.Enemy; } }

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
            entity.GetOrAdd<DataDescriptionComponent>().Text += BuildDataDescription;
        }

        public string BuildDataDescription{
            get {
                StringBuilder sb = new StringBuilder();
                sb.NewLine();
                sb.Append("Damage: ");
                sb.Append(_template.Damage.Min.ToString("F0"));
                sb.Append("-");
                sb.Append(_template.Damage.Max.ToString("F0"));
                if (PrefixAttack != null && !string.IsNullOrEmpty(PrefixAttack.ActionDescription(this))) {
                    sb.Append(" + ");
                    sb.Append(PrefixAttack.ActionDescription(this));
                }
                if (SuffixAttack != null && !string.IsNullOrEmpty(SuffixAttack.ActionDescription(this))) {
                    sb.Append(" + ");
                    sb.Append(SuffixAttack.ActionDescription(this));
                }
                sb.NewLine();
                sb.Append("ToHit: ");
                sb.Append(_template.ToHitBonus.ToString("F0"));
                sb.NewLine();
                sb.Append("Crit: ");
                sb.Append(_template.CritChance.ToString("F0"));
                sb.NewLine();
                sb.Append("Crit Multi: ");
                sb.Append(_template.CritMultiplier.ToString("F0"));
                return sb.ToString();
            }
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

        public bool UnjamWeapon(int maxJamLvl) {
            if (maxJamLvl >= (int) Template.TechLevel) {
                _jammed = false;
                this.GetEntity().Post(new StatusUpdate("Weapon Cleared", Color.green));
                return true;
            }
            return false;
        }

        public void Jam() {
            _jammed = true;
            this.GetEntity().Post(new StatusUpdate("Weapon Jammed", Color.red));
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
