using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class ActionWeaponRequirement : IActionRequirement {

        public enum Types {
            Melee,
            Ranged,
            Unarmed,
            Any
        }

        private Types _type;

        public ActionWeaponRequirement(Types type) {
            _type = type;
        }

<<<<<<< HEAD
        public string Description(BaseActionTemplate template, CharacterTemplate character) {
=======
        public string Description(ActionTemplate template, CharacterTemplate character) {
>>>>>>> FirstPersonAction
            switch (_type) {
                case Types.Melee:
                    return "Requires: Melee Weapon";
                case Types.Ranged:
                    return "Requires: Ranged Weapon";
                case Types.Unarmed:
                    return "Requires: Unarmed";
    
            }
            return "";
        }

<<<<<<< HEAD
        public bool CanTarget(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            var weapon = character.EquipmentSlots.GetSlot(EquipmentSlotType.Weapon).Item;
=======
        public bool CanTarget(ActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            var weapon = character.EquipmentSlots.GetSlot(EquipmentSlotTypes.MainHand).Item;
>>>>>>> FirstPersonAction
            if (weapon == null) {
                return _type == Types.Unarmed;
            }
            if (_type == Types.Any) {
                return true;
            }
            var data = weapon.Get<GenericDataComponent>();
            if (data == null) {
                return false;
            }
            var weaponType = data.GetString(GenericDataTypes.WeaponType) ;
            switch (_type) {
                case Types.Melee:
                    return weaponType == WeaponTypes.Melee;
                case Types.Ranged:
                    return weaponType == WeaponTypes.Ranged;
            }
            return true;
        }

<<<<<<< HEAD
        public bool CanEffect(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
=======
        public bool CanEffect(ActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
>>>>>>> FirstPersonAction
            return true;
        }
    }
}