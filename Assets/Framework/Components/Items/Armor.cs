using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    public class Armor : IComponent, IReceive<EquipmentChanged> {
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
                this.GetEntity().GetOrAdd<DataDescriptionComponent>().Text += BuildDataDescription();
            }
        }

        private float[] _defense;

        private string[] _defModIds = new string[4];
        private GenericStats _current;
        private List<IEntityModifier> _otherMods = new List<IEntityModifier>();

        public List<IEntityModifier> OtherMods { get { return _otherMods; } }
        public float[] Defense { get { return _defense; } }

        public Armor(int numDmgTypes, int level) {
            _defense = new float[numDmgTypes];
        }

        protected string BuildDataDescription() {
            var sb = new StringBuilder();
            sb.NewLine();
            for (int i = 0; i < _defense.Length; i++) {
                if (Math.Abs(_defense[i]) > 0.1f) {
                    sb.Append("<b>");
                    sb.Append(DamageTypes.GetDescriptionAt(i));
                    sb.Append(" Defense</b>: ");
                    sb.NewLineAppend(_defense[i].ToString("F0"));
                }
            }
            for (int i = 0; i < _otherMods.Count; i++) {
                sb.Append("<b>Bonus:</b> ");
                sb.NewLineAppend(_otherMods[i].Description);
            }
            return sb.ToString();
        }


        public void Handle(EquipmentChanged arg) {
            if (_current != null) {
                Clear();
            }
            if (arg.Owner != null) {
                _current = arg.Owner.Get<GenericStats>();
            }
            if (_current != null) {
                Equip();
            }
        }

        public void Equip() {
            for (int i = 0; i < _defense.Length; i++) {
                if (Math.Abs(_defense[i]) < 0.1f) {
                    _defModIds[i] = "";
                    continue;
                }
                _defModIds[i] = _current.Get(DamageTypes.GetIdAt(i)).AddValueMod(_defense[i]);
            }
            var modContainer = _current.Get<ModifiersContainer>();
            if (modContainer == null) {
                return;
            }
            var owner = _current.GetEntity();
            for (int i = 0; i < _otherMods.Count; i++) {
                modContainer.AttachMod(_otherMods[i], owner);
            }
        }

        private void Clear(){
            if (_current == null) {
                return;
            }
            for (int i = 0; i < _defModIds.Length; i++) {
                if (string.IsNullOrEmpty(_defModIds[i])) {
                    continue;
                }
                _current.Get(DamageTypes.GetIdAt(i)).RemoveMod(_defModIds[i]);
                _defModIds[i] = "";
            }
            var modContainer = _current.Get<ModifiersContainer>();
            if (modContainer != null) {
                for (int i = 0; i < _otherMods.Count; i++) {
                    modContainer.RemoveMod(_otherMods[i].Id);
                }
            }
            _current = null;
        }
    }
}
