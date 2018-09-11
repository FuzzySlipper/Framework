using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    public abstract class ItemEquipmentModifier : ItemModifier {

        public abstract void EquipDescription(Equipment item, StringBuilder sb);
        public abstract void SetEquipped(Equipment item, bool equipped);
    }
}
