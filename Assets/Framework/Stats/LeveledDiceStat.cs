﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class LeveledDiceStat : DiceStat {
        public List<LeveledValue> LeveledValues = new List<LeveledValue>();

        public LeveledDiceStat(int entity, string label, float baseValue, int diceRolls, int diceSides) : base(entity, label, baseValue, diceRolls, diceSides) { }
    }

    public struct LeveledValue {
        public int Level;
        public float BonusValue;
        public int BonusRolls;
        public int BonusDice;
    }
}
