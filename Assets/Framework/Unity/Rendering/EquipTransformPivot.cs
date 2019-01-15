using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
namespace PixelComrades {
    public class EquipTransformPivot : MonoBehaviour {
        [ValueDropdown("EquipList")]
        public int SlotType;

        private static ValueDropdownList<int> _equipList;
        private ValueDropdownList<int> EquipList() {
            if (_equipList == null) {
                _equipList = new ValueDropdownList<int>();
                _equipList.Add("Passive", 0);
                _equipList.Add("Active", 1);
                _equipList.Add("Turret", 2);
                _equipList.Add("Forward", 3);
                _equipList.Add("Engine", 4);
                _equipList.Add("PowerPlant", 5);
            }
            return _equipList;
        }


    }
}
