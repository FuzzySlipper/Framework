using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Random = System.Random;


namespace PixelComrades {
    public class RangeBase {}

    [Serializable]
    public class FloatRange : RangeBase {
        public float Min;
        public float Max;

        public FloatRange(float min, float max) {
            Min = min;
            Max = max;
        }

        public FloatRange() {}

        public float MidPoint { get { return (Min + Max) / 2; } }

        public float Clamp(float input) {
            return Mathf.Clamp(input, Min, Max);
        }

        public virtual bool IsInRange(float input) {
            if (input < Min || input > Max) {
                return false;
            }
            return true;
        }

        public virtual bool IsInMargin(float margin, float input) {
            if (input < Min || input > Max) {
                return false;
            }
            if (input < (Min + margin)) {
                return true;
            }
            if (input > (Max - margin)) {
                return true;
            }
            return false;
        }

        public float Get() {
            return Get(Game.Random);
        }

        public float Get(System.Random random) {
            if (Math.Abs(Max) < 0.00001f && Math.Abs(Min) < 0.00001f) {
                return 0;
            }
            if (Max < Min) {
                return random.NextFloat(Max, Min);
            }
            return random.NextFloat(Min, Max);
        }

        public float InverseLerp(float t) {
            return Mathf.InverseLerp(Min, Max, t);
        }

        public float Lerp(float t) {
            return Mathf.Lerp(Min, Max, t);
        }

        public override string ToString() {
            if (Math.Abs(Min - Max) < 0.001f) {
                return Min.ToString("F0");
            }
            return string.Format("{0:F1}-{1:F1}", Min, Max);
        }

        public static FloatRange Parse(string input) {
            if (input.Length < 3) {
                return null;
            }
            var numbers = input.Split('-');
            if (numbers.Length == 0) {
                return null;
            }
            float min;
            if (!float.TryParse(numbers[0], out min)) {
                return null;
            }
            if (numbers.Length == 1) {
                return new FloatRange(min, min);
            }
            float max;
            if (!float.TryParse(numbers[1], out max)) {
                max = min;
            }
            return new FloatRange(min, max);
        }
    }

    [Serializable]
    public class NormalizedFloatRange : FloatRange {
        public NormalizedFloatRange(float min, float max) {
            Min = min;
            Max = max;
        }
        public NormalizedFloatRange() {}
    }

    [Serializable]
    public class IntRange : RangeBase {
        public int Min;
        public int Max;

        public IntRange(int min, int max) {
            Min = min;
            Max = max;
        }

        public IntRange() {
        }

        public int Clamp(int value) {
            return Mathf.Clamp(value, Min, Max);
        }

        public int Lerp(float t) {
            return (int) Mathf.Lerp(Min, Max, t);
        }

        public int Get() {
            if (Min == Max) {
                return Min;
            }
            return Get(Game.Random);
        }

        public int Get(System.Random random) {
            if (Min == Max) {
                return Min;
            }
            return random.Next(Min, Max);
        }

        public bool WithinRange(float value) {
            return Min <= value && Max >= value;
        }

        public override string ToString() {
            return string.Format("{0}-{1}", Min, Max);
        }

        public static IntRange Parse(string input) {
            if (input.Length < 3) {
                return null;
            }
            var numbers = input.Split('-');
            if (numbers.Length == 0) {
                return null;
            }
            int min;
            if (!int.TryParse(numbers[0], out min)) {
                return null;
            }
            if (numbers.Length == 1) {
                return new IntRange(min, min);
            }
            int max;
            if (!int.TryParse(numbers[1], out max)) {
                max = min;
            }
            return new IntRange(min, max);
        }
    }

    [Serializable] public class VectorUniformRange : RangeBase {

        public bool AllowZero = false;
        public float Min;
        public float Max;

        public VectorUniformRange(float min, float max) {
            Min = min;
            Max = max;
        }

        public VectorUniformRange() {
        }

        public Vector3 Get() {
            return Get(Game.Random);
        }

        public Vector3 Get(System.Random random) {
            if (Math.Abs(Max) < 0.001f && Math.Abs(Min) < 0.001f) {
                return Vector3.zero;
            }
            var value = random.NextFloat(Min, Max);
            if (!AllowZero) {
                while (Math.Abs(value) < 0.1f) {
                    value = random.NextFloat(Min, Max);
                }
            }
            return new Vector3(value, value, value);
        }

    }

    [Serializable] public class VectorRange : RangeBase {
        public FloatRange[] Range = new FloatRange[3];

        public VectorRange() {
            Range[0] = new FloatRange(0, 0);
            Range[1] = new FloatRange(0, 0);
            Range[2] = new FloatRange(0, 0);
        }

        public VectorRange(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax) {
            Range[0] = new FloatRange(xMin, xMax);
            Range[1] = new FloatRange(yMin, yMax);
            Range[2] = new FloatRange(zMin, zMax);
        }


        public Vector3 Clamp(Vector3 input) {
            for (var i = 0; i < 2; i++) {
                input[i] = Range[i].Clamp(input[i]);
            }
            return input;
        }

        public Vector3 Get() {
            return Get(Game.Random);
        }

        public Vector3 Get(System.Random random) {
            return new Vector3(Range[0].Get(random), Range[1].Get(random), Range[2].Get(random));
        }

    }
    [Serializable] public class PointRange : RangeBase {

        public IntRange[] Range = new IntRange[3];
        public Point3 Min { get { return new Point3(Range[0].Min, Range[1].Min, Range[2].Min); } }
        public Point3 Max { get { return new Point3(Range[0].Max, Range[1].Max, Range[2].Max); } }

        public PointRange(int xMin, int xMax, int yMin, int yMax, int zMin, int zMax) {
            Range[0] = new IntRange(xMin, xMax);
            Range[1] = new IntRange(yMin, yMax);
            Range[2] = new IntRange(zMin, zMax);
        }

        public PointRange(int min, int max) {
            Range[0] = new IntRange(min, max);
            Range[1] = new IntRange(min, max);
            Range[2] = new IntRange(min, max);
        }

        public Point3 Get() {
            return Get(Game.Random);
        }

        public Point3 Get(System.Random random) {
            return new Point3(Range[0].Get(random), Range[1].Get(random), Range[2].Get(random));
        }

        public bool InRange(Point3 pos) {
            for (int i = 0; i < Range.Length; i++) {
                if (!Range[i].WithinRange(pos[i])) {
                    return false;
                }
            }
            return true;
        }
    }
    [Serializable] public class RandomBool : RangeBase {
        public float Chance = 50;

        public RandomBool(float chance) {
            Chance = chance;
        }

        public bool IsTrue() {
            return IsTrue(Game.Random);
        }

        public bool IsTrue(System.Random random) {
            return random.Next(0, 101) < Chance;
        }
    }
    public class VectorRangeCollection {

        private HashSet<Point3> _positions = new HashSet<Point3>();

        private IntRange[] _range = new IntRange[3];
        //private bool _started = false;

        public VectorRangeCollection() {
            for (var i = 0; i < _range.Length; i++) {
                _range[i] = new IntRange();
                _range[i].Min = int.MaxValue;
                _range[i].Max = int.MinValue;
            }
        }

        public bool Contains(Point3 point) {
            return _positions.Contains(point);
        }

        public int Count { get { return _positions.Count; } }
        public IntRange this[int index] { get { return _range[index]; } }
        public Vector3 min { get { return new Vector3(_range[0].Min, _range[1].Min, _range[2].Min); } }
        public Vector3 max { get { return new Vector3(_range[0].Max, _range[1].Max, _range[2].Max); } }
        public IntRange X { get { return _range[0]; } }
        public IntRange Y { get { return _range[1]; } }
        public IntRange Z { get { return _range[2]; } }

        public Vector3 MaxVector3 { get { return new Vector3(max.x, max.y, max.z); } }

        public void AddPosition(Point3 pos) {
            for (var i = 0; i < _range.Length; i++) {
                if (pos[i] < _range[i].Min) {
                    _range[i].Min = pos[i];
                }
                if (pos[i] > _range[i].Max) {
                    _range[i].Max = pos[i];
                }
            }
            _positions.Add(pos);
        }

        public void Clear() {
            _positions.Clear();
            for (var i = 0; i < _range.Length; i++) {
                _range[i].Min = int.MaxValue;
                _range[i].Max = int.MinValue;
            }
        }

        public void RemovePosition(Point3 oldPos) {
            _positions.Remove(oldPos);
            for (var i = 0; i < _range.Length; i++) {
                if (oldPos[i] <= _range[i].Min) {
                    ResetMinMax();
                    break;
                }
                if (oldPos[i] >= _range[i].Max) {
                    ResetMinMax();
                    break;
                }
            }
        }

        private void ResetMinMax() {
            for (var i = 0; i < _range.Length; i++) {
                _range[i].Min = int.MaxValue;
                _range[i].Max = int.MinValue;
            }
            var enumerator = _positions.GetEnumerator();
            try {
                while (enumerator.MoveNext()) {
                    var pos = enumerator.Current;
                    for (var i = 0; i < _range.Length; i++) {
                        if (pos[i] < _range[i].Min) {
                            _range[i].Min = pos[i];
                        }
                        if (pos[i] > _range[i].Max) {
                            _range[i].Max = pos[i];
                        }
                    }
                }
            }
            finally {
                enumerator.Dispose();
            }
        }
    }

    [Serializable]
    public class ChanceHolder : RangeBase {

        [SerializeField, Range(0, 100)] private int _chance;

        public float Chance { get { return _chance; } set { _chance = (int)value; } }

        public ChanceHolder(int chance) {
            //_chance = Mathf.Clamp(chance, 0, 100);
            _chance = chance;
        }

        public ChanceHolder() {}

        public bool RollSuccessfull() {
            return RollSuccessfull(Game.Random);
        }

        public bool RollSuccessfull(System.Random random) {
            return random.DiceRollSucess(_chance);
        }

        public override string ToString() {
            return string.Format("Chance {0:F1}", _chance);
        }
    }
}