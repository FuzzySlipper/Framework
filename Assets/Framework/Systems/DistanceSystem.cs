using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vector3 = UnityEngine.Vector3;

namespace PixelComrades {
    public class DistanceSystem : SystemBase {

        public static float GetDistance(Entity source, Entity target) {
            return Vector3.Distance(source.GetPosition(), target.GetPosition());
        }

        public static float GetDistance(Entity source, Vector3 target) {
            return Vector3.Distance(source.GetPosition(), target);
        }

        public static float GetDistance(Vector3 source, Vector3 target) {
            return Vector3.Distance(source, target);
        }
    }
}
