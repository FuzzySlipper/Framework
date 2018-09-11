using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public partial class EquipSlotType : GenericEnum<EquipSlotType, int> {
        public override int Parse(string value, int defaultValue) {
            if (int.TryParse(value, out var val)) {
                return val;
            }
            return TryValueOf(value, out val) ? val : defaultValue;
        }
    }
}
