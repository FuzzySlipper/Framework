using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class ActionDataStringRequirement : IActionRequirement {

        private string _data;
        private string _label;
        private ActionDataTargetType _type;

        public ActionDataStringRequirement(string data, string label, ActionDataTargetType type) {
            _data = data;
            _label = label;
            _type = type;
        }

        public string Description(BaseActionTemplate template, CharacterTemplate character) {
            return string.Format("Requires: {0}", _label);
        }

        public bool CanTarget(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            GenericDataComponent data = null;
            switch (_type) {
                case ActionDataTargetType.Action:
                    data = template.Data;
                    break;
                case ActionDataTargetType.Target:
                    data = character.Data;
                    break;
                case ActionDataTargetType.Owner:
                    data = target.Data;
                    break;
            }
            return data != null && data.HasString(_data);
        }

        public bool CanEffect(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            if (_type == ActionDataTargetType.Target) {
                var data = target.Data;
                return data != null && data.HasString(_data);
            }
            return true;
        }
    }

    public sealed class ActionDataIntRequirement : IActionRequirement {

        private string _data;
        private int _minAmount;
        private string _label;
        private ActionDataTargetType _type;

        public ActionDataIntRequirement(string data, int minAmount, string label, ActionDataTargetType type) {
            _data = data;
            _minAmount = minAmount;
            _label = label;
            _type = type;
        }

        public string Description(BaseActionTemplate template, CharacterTemplate character) {
            return string.Format("Requires: {0}", _label);
        }

        public bool CanTarget(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            GenericDataComponent data = null;
            switch (_type) {
                case ActionDataTargetType.Action:
                    data = template.Data;
                    break;
                case ActionDataTargetType.Target:
                    data = character.Data;
                    break;
                case ActionDataTargetType.Owner:
                    data = target.Data;
                    break;
            }
            if (data == null) {
                return false;
            }
            return data.GetInt(_data) >= _minAmount;
        }

        public bool CanEffect(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            if (_type == ActionDataTargetType.Target) {
                var data = target.Data;
                return data != null && data.GetInt(_data) >= _minAmount;
            }
            return true;
        }
    }

    public enum ActionDataTargetType {
        Owner,
        Target,
        Action
    }
}