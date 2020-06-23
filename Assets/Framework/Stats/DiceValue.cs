using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public struct DiceValue {
        public int DiceRolls;
        public DiceSides DiceSides;
        public int Bonus;

        public DiceValue(int diceRolls, DiceSides diceSides, int bonus) {
            DiceRolls = diceRolls;
            DiceSides = diceSides;
            Bonus = bonus;
        }

        public DiceValue(int diceRolls, DiceSides diceSides) {
            DiceRolls = diceRolls;
            DiceSides = diceSides;
            Bonus = 0;
        }

        public override string ToString() {
            return string.Format("{0}d{1}+{2}", DiceRolls, DiceSides, Bonus);
        }
    }
}
