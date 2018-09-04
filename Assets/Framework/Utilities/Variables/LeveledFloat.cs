using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public class LeveledFloat {
        public float BaseValue;
        public float PerLevel;

        public LeveledFloat(float val, float lvl) {
            BaseValue = val;
            PerLevel = lvl;
        }

        public LeveledFloat(){}

        public float Get(int level) {
            return BaseValue + (PerLevel * level);
        }

        public static LeveledFloat Parse(string input) {
            if (input.Length < 3) {
                return null;
            }
            var numbers = input.Split('+');
            if (numbers.Length == 0) {
                return null;
            }
            float baseVal;
            if (!float.TryParse(numbers[0], out baseVal)) {
                return null;
            }
            if (numbers.Length == 1) {
                return new LeveledFloat(baseVal, 0);
            }
            float perLevel;
            float.TryParse(numbers[1], out perLevel);
            return new LeveledFloat(baseVal, perLevel);
        }
    }

    [System.Serializable]
    public class LeveledFloatRange {
        public float BaseValue;
        public float PerLevel;
        public float Multi;

        public LeveledFloatRange(float val, float lvl, float multi) {
            BaseValue = val;
            PerLevel = lvl;
            Multi = multi;
        }

        public LeveledFloatRange() {
        }

        public FloatRange Get(int level) {
            var val = BaseValue + (PerLevel * level);
            return new FloatRange(val, val * Multi);
        }

        public static LeveledFloatRange Parse(string input) {
            if (input.Length < 3) {
                return null;
            }
            var numbers = input.Split('+');
            if (numbers.Length == 0) {
                return null;
            }
            float baseVal;
            if (!float.TryParse(numbers[0], out baseVal)) {
                return null;
            }
            if (numbers.Length == 1) {
                return new LeveledFloatRange(baseVal, 0, 1);
            }
            float perLevel;
            float.TryParse(numbers[1], out perLevel);
            if (numbers.Length == 2) {
                return new LeveledFloatRange(baseVal, perLevel, 1);
            }
            float multi;
            if (!float.TryParse(numbers[2], out multi)) {
                multi = 1;
            }
            return new LeveledFloatRange(baseVal, perLevel, multi);
        }
    }
}
