using System.Collections;
using System.Collections.Generic;
using System;

namespace PixelComrades {
    
    public enum ImpactTypes {
        None,
        Damage,
        Heal,
        LeachVital,
        Special,
    }

    public enum ImpactRadiusTypes {
        Single = 0,
        Cone = 10,
        Cross = 20,
        Cross2 = 21,
        Line = 22,
        Radius1 = 30,
        Radius2 = 31,
        Party = 40,
    }

    public static class ImpactExtensions {
        
        public static List<Point3> RadiusPoints(this ImpactRadiusTypes radius, Point3 center, Directions fwd) {
            if (radius == ImpactRadiusTypes.Single) {
                return null;
            }
            switch (radius) {
                default:
                case ImpactRadiusTypes.Single:
                    return null;
                case ImpactRadiusTypes.Cross:
                    return RadiusPnts(_surrounding4Way, center, 1);
                case ImpactRadiusTypes.Cross2:
                    return RadiusPnts(_surrounding4Way, center, 2);
                case ImpactRadiusTypes.Radius1:
                    return RadiusPnts(_surrounding8Way, center, 2);
                case ImpactRadiusTypes.Radius2:
                    return RadiusPnts(_surrounding8Way, center, 3);
                case ImpactRadiusTypes.Cone:
                    return GetOctantPnts(center, 2, fwd);
            }
        }

        private static List<Point3> GetOctantPnts(Point3 center, int radius, Directions fwd) {
            List<Point3> pnts = new List<Point3>();
            var octants = ShadowCastingCalculation.OctantsFromDir(fwd);
            for (int i = 0; i < octants.Length; i++) {
                ShadowCastingCalculation.GetVisiblePoints(center, octants[i], radius, ref pnts);
            }
            return pnts;
        }

        private static List<Point3> RadiusPnts(Point3[] pntArray, Point3 center, int radius) {
            List<Point3> pnts = new List<Point3>();
            for (int p = 0; p < pntArray.Length; p++) {
                var dir = pntArray[p];
                for (int r = 1; r <= radius; r++) {
                    var pos = center + (dir * r);
                    pnts.Add(pos);
                }
            }
            return pnts;
        }

        private static Point3[] _surrounding4Way = {
            new Point3(1,0,0), new Point3(-1,0,0),
            new Point3(0,0,1), new Point3(0,0,-1),
        };

        //private static Point3[] _surroundingUpDown = {
        //    new Point3(1,0,0), new Point3(-1,0,0),
        //    new Point3(0,1,0), new Point3(0,-1,0),
        //    new Point3(0,0,1), new Point3(0,0,-1),
        //};

        private static Point3[] _surrounding8Way = {
            new Point3(-1, 0, 1), new Point3(0, 0, 1), new Point3(1, 0, 1), new Point3(-1, 0, 0), 
            new Point3(-1, 0, -1), new Point3(0, 0, -1), new Point3(1, 0, -1), new Point3(1, 0, 0), //(0,0,0) is self
        };
    }

    public static class ShadowCastingCalculation {

        private static List<Shadow> _shadows = new List<Shadow>();

        public static void GetVisibleCells(Point3 start, int octant, int maxRowDistance, Action<BaseCell, float> del) {
            if (del == null) {
                return;
            }
            Point3 rowInc;
            Point3 colInc;
            ApplyOctant(octant, out colInc, out rowInc);
            var startCell = World.Get<MapSystem>().GetCell(start);
            if (startCell != null) {
                del(startCell, 1);
            }
            _shadows.Clear();
            var fullShadow = false;
            for (var row = 1; row < maxRowDistance; row++) {
                var currentPos = start + rowInc * row;
                for (var col = 0; col <= row; col++) {
                    var cell = World.Get<MapSystem>().GetCell(currentPos);
                    var blocksLight = false;
                    var isVisible = false;
                    Shadow projection = null;
                    if (!fullShadow) {
                        projection = GetProjection(col, row);
                        isVisible = !IsInShadow(projection, ref _shadows);
                        if (cell == null) {
                            blocksLight = true;
                            isVisible = false;
                        }
                        else {
                            var colAdjacent = currentPos - colInc;
                            blocksLight = cell.BlocksVision(currentPos.GetTravelDirTo(colAdjacent));
                            if (!blocksLight) {
                                var rowAdjacent = currentPos - rowInc;
                                blocksLight = cell.BlocksVision(currentPos.GetTravelDirTo(rowAdjacent));
                            }
                        }
                    }
                    if (blocksLight) {
                        fullShadow = AddShadow(projection, ref _shadows);
                    }
                    if (isVisible) {
                        del(cell, 1);
                    }
                    currentPos += colInc;
                }
            }
        }

        public static void GetVisiblePoints(Point3 start, int octant, int maxRowDistance, ref List<Point3> visiblePoints) {
            Point3 rowInc;
            Point3 colInc;
            ApplyOctant(octant, out colInc, out rowInc);
            if (visiblePoints.Contains(start)) {
                visiblePoints.Add(start);
            }
            for (var row = 1; row < maxRowDistance; row++) {
                var currentPos = start + rowInc * row;
                for (var col = 0; col <= row; col++) {
                    if (!visiblePoints.Contains(currentPos)) {
                        visiblePoints.Add(currentPos);
                    }
                    currentPos += colInc;
                }
            }
        }

        /// <summary>
        ///     Represents the 1D projection of a 2D shadow onto a normalized line. In other words,
        ///     a range from 0.0 to 1.0.
        /// </summary>
        public class Shadow {
            public Shadow(float start, float end) {
                Start = start;
                End = end;
            }

            public float Start { get; private set; }
            public float End { get; private set; }

            public bool Contains(Shadow projection) {
                return (Start <= projection.Start) && (End >= projection.End);
            }

            public override string ToString() {
                return "(" + Start + "-" + End + ")";
            }

            public void Unify(float start, float end) {
                // see if the shadow overlaps to the right
                if (start <= End) {
                    End = Math.Max(End, end);
                }

                // see if the shadow overlaps to the left
                if (Start <= end) {
                    Start = Start < start ? Start : start;
                }
            }
        }

        private static bool AddShadow(Shadow shadow, ref List<Shadow> sShadows) {
            var index = 0;
            for (index = 0; index < sShadows.Count; index++) {
                // see if we are at the insertion point for this shadow
                if (sShadows[index].Start > shadow.Start) {
                    // break out and handle inserting below
                    break;
                }
            }

            // the new shadow is going here. see if it overlaps the previous or next shadow
            var overlapsPrev = false;
            var overlapsNext = false;

            if ((index > 0) && (sShadows[index - 1].End > shadow.Start)) {
                overlapsPrev = true;
            }

            if ((index < sShadows.Count) && (sShadows[index].Start < shadow.End)) {
                overlapsNext = true;
            }

            // insert and unify with overlapping shadows
            if (overlapsNext) {
                if (overlapsPrev) {
                    // overlaps both, so unify one and delete the other
                    sShadows[index - 1].Unify(shadow.Start, sShadows[index].End);
                    sShadows.RemoveAt(index);
                }
                else {
                    // just overlaps the next shadow, so unify it with that
                    sShadows[index].Unify(shadow.Start, shadow.End);
                }
            }
            else {
                if (overlapsPrev) {
                    // just overlaps the previous shadow, so unify it with that
                    sShadows[index - 1].Unify(shadow.Start, shadow.End);
                }
                else {
                    // does not overlap anything, so insert
                    sShadows.Insert(index, shadow);
                }
            }

            // see if we are now shadowing everything
            return (sShadows.Count == 1) && (sShadows[0].Start == 0) && (sShadows[0].End == 1.0f);
        }

        /// <summary>
        ///     Creates a <see cref="Shadow" /> that corresponds to the projected silhouette
        ///     of the given tile. This is used both to determine visibility (if any of
        ///     the projection is visible, the tile is) and to add the tile to the shadow
        ///     map.
        /// </summary>
        private static Shadow GetProjection(int col, int row) {
            // the bottom edge of row 0 is 1 wide
            float rowBottomWidth = row + 1;

            // the top edge of row 0 is 2 wide
            float rowTopWidth = row + 2;

            // unify the bottom and top edges of the tile
            var start = MathEx.Min(col / rowBottomWidth, col / rowTopWidth);
            var end = MathEx.Max((col + 1.0f) / rowBottomWidth, (col + 1.0f) / rowTopWidth);

            return new Shadow(start, end);
        }

        private static bool IsInShadow(Shadow projection, ref List<Shadow> sShadows) {
            // check the shadow list
            for (int i = 0; i < sShadows.Count; i++) {
                if (sShadows[i].Contains(projection)) {
                    return true;
                }
            }
            return false;
        }

        public static int[] OctantsFromDir(Directions dir) {
            return _dirsToOctant[dir];
        }

        private static Dictionary<Directions, int[]> _dirsToOctant = new Dictionary<Directions, int[]>() {
            {Directions.Back, new int[] {0,7} }, {Directions.Right, new int[] {1,2} },
            {Directions.Left, new int[] {5,6} }, {Directions.Forward, new int[] {3,4} },
        };

        private static void ApplyOctant(int octant, out Point3 colInc, out Point3 rowInc) {
            rowInc = Point3.zero;
            colInc = Point3.zero;

            // figure out which direction to increment based on the octant
            // octant 0 starts at 6 o'clock, and octants proceed clockwise from there
            switch (octant) {
                case 0: //SE
                    rowInc.z = -1;
                    colInc.x = 1;
                    break;
                case 1://ES
                    rowInc.x = 1;
                    colInc.z = -1;
                    break;
                case 2://EN
                    rowInc.x = 1;
                    colInc.z = 1;
                    break;
                case 3://NE
                    rowInc.z = 1;
                    colInc.x = 1;
                    break;
                case 4://NW
                    rowInc.z = 1;
                    colInc.x = -1;
                    break;
                case 5://WN
                    rowInc.x = -1;
                    colInc.z = 1;
                    break;
                case 6://WS
                    rowInc.x = -1;
                    colInc.z = -1;
                    break;
                case 7: //SW
                    rowInc.z = -1;
                    colInc.x = -1;
                    break;
            }
        }
    }
}
