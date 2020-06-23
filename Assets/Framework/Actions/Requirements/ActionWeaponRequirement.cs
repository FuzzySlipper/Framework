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

        public string Description(BaseActionTemplate template, CharacterTemplate character) {
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

        public bool CanTarget(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            var weapon = character.EquipmentSlots.GetSlot(EquipmentSlotType.Weapon).Item;
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

        public bool CanEffect(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            return true;
        }
    }
}