using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class ItemAttackModifier : ItemModifier {

        public virtual string ActionDescription(Weapon weapon) {
            return "";
        }
    }
}
