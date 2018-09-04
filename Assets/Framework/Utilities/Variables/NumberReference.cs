using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    [System.Serializable]
    public class NumberReference {
        public ValueModifier Modifier = ValueModifier.Absolute;
        public virtual string StringValue { get { return ""; } }
    }

    [System.Serializable]
    public class FloatReference : NumberReference {

        public float ConstantMin;
        public float ConstantMax;
        public FloatVariable Variable;

        public FloatRangeVariable RangeVariable { get { return Variable as FloatRangeVariable; } }

        public FloatReference() {
        }

        public FloatReference(float min) {
            Modifier = ValueModifier.Absolute;
            ConstantMin = min;
            ConstantMax = min;
        }

        public FloatReference(float min, float max) {
            Modifier = ValueModifier.Absolute;
            ConstantMin = min;
            ConstantMax = max;
        }

        public override string StringValue {
            get {
                if (RangeVariable == null) {
                    return FlatValue.ToString("F1");
                }
                if (Math.Abs(MinValue - MaxValue) < 0.01f) {
                    return MinValue.ToString("F1");
                }
                return string.Format("{0:F1}-{1:F1}", MinValue, MaxValue);
            }
        }

        public void Set(float value) {
            Modifier = ValueModifier.Absolute;
            ConstantMin = ConstantMax = value;
        }

        public float Value {
            get {
                if (RangeVariable == null) {
                    return FlatValue;
                }
                if (Math.Abs(MinValue - MaxValue) < 0.01f) {
                    return MinValue;
                }
                return Game.Random.NextFloat(MinValue, MaxValue);
            }
        }

        private float FlatValue {
            get {
                if (Variable == null) {
                    return ConstantMin;
                }
                switch (Modifier) {
                    case ValueModifier.Reference:
                        return Variable.Value;
                    case ValueModifier.Absolute:
                        return ConstantMin;
                    case ValueModifier.Multiply:
                        return Variable.Value * ConstantMin;
                    case ValueModifier.Add:
                        return Variable.Value + ConstantMin;
                    case ValueModifier.Percent:
                        return ((ConstantMin * 0.01f) * Variable.Value);
                }
                return Variable.Value;
            }
        }

        public float MinValue {
            get {
                if (Variable == null) {
                    return ConstantMin;
                }
                if (RangeVariable == null) {
                    return FlatValue;
                }
                switch (Modifier) {
                    case ValueModifier.Reference:
                        return RangeVariable.Min;
                    case ValueModifier.Absolute:
                        return ConstantMin;
                    case ValueModifier.Multiply:
                        return RangeVariable.Min * ConstantMin;
                    case ValueModifier.Add:
                        return RangeVariable.Min + ConstantMin;
                    case ValueModifier.Percent:
                        return ((ConstantMin * 0.01f) * RangeVariable.Min);
                }
                return RangeVariable.Min;
            }
        }

        public float MaxValue {
            get {
                if (Variable == null) {
                    return ConstantMax;
                }
                if (RangeVariable == null) {
                    return FlatValue;
                }
                switch (Modifier) {
                    case ValueModifier.Reference:
                        return RangeVariable.Max;
                    case ValueModifier.Absolute:
                        return ConstantMax;
                    case ValueModifier.Multiply:
                        return RangeVariable.Max * ConstantMax;
                    case ValueModifier.Add:
                        return RangeVariable.Max + ConstantMax;
                    case ValueModifier.Percent:
                        return ((ConstantMax * 0.01f) * RangeVariable.Max);
                }
                return RangeVariable.Max;
            }
        }

        public string StringValueMultiplied(float amount) {
            if (RangeVariable == null) {
                return (amount * FlatValue).ToString("F1");
            }
            if (Math.Abs(MinValue - MaxValue) < 0.01f) {
                return (amount * MinValue).ToString("F1");
            }
            return string.Format("{0:F1}-{1:F1}", amount * MinValue, amount * MaxValue);
        }

        public static implicit operator float(FloatReference reference) {
            return reference.Value;
        }
    }

    [System.Serializable]
    public class IntReference : NumberReference {

        public int ConstantMin;
        public IntVariable Variable;

        public IntReference() {
        }

        public IntReference(int value) {
            Modifier = ValueModifier.Absolute;
            ConstantMin = value;
        }

        public override string StringValue { get { return Value.ToString(); } }

        public int Value {
            get {
                if (Variable == null) {
                    return ConstantMin;
                }
                switch (Modifier) {
                    case ValueModifier.Reference:
                        return Variable.Value;
                    case ValueModifier.Absolute:
                        return ConstantMin;
                    case ValueModifier.Multiply:
                        return Variable.Value * ConstantMin;
                    case ValueModifier.Add:
                        return Variable.Value + ConstantMin;
                    case ValueModifier.Percent:
                        return (int)((ConstantMin * 0.01f) * Variable.Value);
                }
                return Variable.Value;
            }
        }

        public static implicit operator int(IntReference reference) {
            return reference.Value;
        }
    }
}
