using UnityEngine;
using System.Collections;

namespace PixelComrades {
    [System.Serializable] public class ProbabilisticCurve {

        [Tooltip("Should be 0-1 Range")] public AnimationCurve Curve;
        public FloatRange Range = new FloatRange(0, 1);

        public float Get {
            get {
                float x = Random.value;
                float y = Curve.Evaluate(x);
                // Scale it to be between your max and min
                return Range.Min + (y * (Range.Max - Range.Min));
            }
        }
    }
}