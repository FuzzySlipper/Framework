using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public class ActionHolder {
        public int LastRoll { get; private set; }
        public int Chance; // this needs to be in range 0 -100
        public ActionConfig Action;
        public AdvancedTargeting Targeting;

        public ActionHolder(int chanceNotNormalized, ActionConfig actionConfig, AdvancedTargeting targeting) {
            Chance = chanceNotNormalized;
            Action = actionConfig;
            Targeting = targeting;
            LastRoll = 0;
        }

        public void Roll() {
            LastRoll = Game.Random.Next(0, Chance);
        }
    }
}
