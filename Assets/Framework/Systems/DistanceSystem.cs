using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vector3 = UnityEngine.Vector3;

namespace PixelComrades {
    public class DistanceSystem : SystemBase {

        public static float GetUnitDistance2D(Entity source, Entity target) {
            var pos1 = source.GetPosition().ToUnitGridZeroY();
            var pos2 = target.GetPosition().ToUnitGridZeroY();
            return pos1.DistanceCheb(pos2);
        }

        public static float GetUnitDistance3D(Entity source, Entity target) {
            var pos1 = source.GetPosition().ToUnitGrid();
            var pos2 = target.GetPosition().ToUnitGrid();
            return pos1.DistanceCheb(pos2);
        }

        public static float GetDistance(Entity source, Vector3 target) {
            return Vector3.Distance(source.GetPosition(), target);
        }

        public static float GetDistance(Vector3 source, Vector3 target) {
            return Vector3.Distance(source, target);
        }

        public static float FromUnitGridDistance(int unitDistance) {
            return unitDistance * GameConstants.UnitGrid;
        }

        public static int ToUnitGridDistance(float dist) {
            return (int) System.Math.Round((double) dist / GameConstants.UnitGrid);
        }
    }
}
