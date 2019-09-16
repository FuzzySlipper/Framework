using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class RuleSystem : SystemBase {
        
        public static float GetDefenseAmount(float damage, float stat) {
            return damage * ((stat / (damage * 10)) * 0.5f);
        }
    }
}
