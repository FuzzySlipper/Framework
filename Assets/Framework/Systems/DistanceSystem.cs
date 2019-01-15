using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vector3 = UnityEngine.Vector3;

namespace PixelComrades {
    public class DistanceSystem : SystemBase {

        public static float GetDistance(Entity source, Entity target) {
            //return Vector3.Distance(source.GetPosition(), target.GetPosition());
            var pos1 = source.GetPosition();
            var pos2 = target.GetPosition();
            pos1.y = 0;
            pos2.y = 0;
            return Vector3.Distance(pos1, pos2);
        }

        public static float GetDistance(Entity source, Vector3 target) {
            return Vector3.Distance(source.GetPosition(), target);
        }

        public static float GetDistance(Vector3 source, Vector3 target) {
            return Vector3.Distance(source, target);
        }
    }
}
