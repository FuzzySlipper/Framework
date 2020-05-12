using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ActionRangeRequirement : IActionRequirement {

        private int _range;

        public ActionRangeRequirement(int range) {
            _range = range;
        }

        public string Description(BaseActionTemplate template, CharacterTemplate character) {
            return string.Format("Range: {0}", _range);
        }

        public bool CanTarget(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            return character.Position.Value.DistanceCheb(target.Position.Value) <= _range;
        }

        public bool CanEffect(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            return true;
        }
    }

    public class ActionWeaponRangeRequirement : IActionRequirement {

        private bool _isMelee;

        public ActionWeaponRangeRequirement(bool isMelee) {
            _isMelee = isMelee;
        }

        public string Description(BaseActionTemplate template, CharacterTemplate character) {
            return string.Format("Range: {0} Weapon", _isMelee ? "Melee" : "Ranged");
        }

        public bool CanTarget(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            return character.Position.Value.DistanceCheb(target.Position.Value) <= character.Stats.GetValue(Stats.WeaponAttackRange);
        }

        public bool CanEffect(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            return true;
        }
    }

    public class ActionTouchRangeRequirement : IActionRequirement {


        public string Description(BaseActionTemplate template, CharacterTemplate character) {
            return "Range: Touch";
        }

        public bool CanTarget(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            return character.Position.Value.DistanceCheb(target.Position.Value) <= character.Stats.GetValue(Stats.Reach);
        }

        public bool CanEffect(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            return true;
        }
    }

    public class ActionSelfRangeRequirement : IActionRequirement {


        public string Description(BaseActionTemplate template, CharacterTemplate character) {
            return "Range: Self";
        }

        public bool CanTarget(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            return character == target;
        }

        public bool CanEffect(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            return true;
        }
    }
}
