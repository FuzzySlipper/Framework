using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public class ActionHolder {
        public int LastRoll { get; private set; }
        public int Chance; // this needs to be in range 0 -100
        public ActionTemplate Action;
        public AdvancedTargeting Targeting;

        public ActionHolder(int chanceNotNormalized, Entity actionEntity, AdvancedTargeting targeting) {
            Chance = chanceNotNormalized;
            if (actionEntity != null) {
                Action = actionEntity.GetTemplate<ActionTemplate>();
            }
            Targeting = targeting;
            LastRoll = 0;
        }

        public void Roll() {
            LastRoll = Game.Random.Next(0, Chance);
        }
    }

    [System.Serializable]
    public class AbilityHolder {
<<<<<<< HEAD
        [Range(0, 100)] public int Chance;
=======
        [Range(0,100)] public int Chance;
>>>>>>> FirstPersonAction
        public AbilityConfig Ability;
        public AdvancedTargeting Targeting;
    }
}
