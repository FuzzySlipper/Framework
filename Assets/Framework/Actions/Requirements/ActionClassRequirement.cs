using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class ActionClassRequirement : IActionRequirement {

        private string _classId;
        private string _label;
        private int _minLevel;

        public ActionClassRequirement(string classId, int minLevel = 1) {
            _classId = classId;
            _minLevel = minLevel;
            _label = GameData.Classes.GetNameAt(classId);
        }

        public string Description(BaseActionTemplate template, CharacterTemplate character) {
            if (_minLevel > 1) {
                return string.Format("Requires: {0} {1}", _minLevel, _label);
            }
            return string.Format("Requires: {0}", _label);
        }

        public bool CanTarget(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            return character.GenericData.GetInt(_classId) >= _minLevel;
        }

        public bool CanEffect(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            return true;
        }
    }
}