using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public class ActionHolder {
        public int LastRoll { get; private set; }
        public int Chance; // this needs to be in range 0 -100
        public Action Action;
        public AdvancedTargeting Targeting;

        public ActionHolder(int chanceNotNormalized, Action action, AdvancedTargeting targeting) {
            Chance = chanceNotNormalized;
            Action = action;
            Targeting = targeting;
            LastRoll = 0;
        }

        public void Roll() {
            LastRoll = Game.Random.Next(0, Chance);
        }
    }
}
