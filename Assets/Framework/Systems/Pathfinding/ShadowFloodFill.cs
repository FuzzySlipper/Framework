using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ShadowFloodFill {

        public static void GetVisiblePoints(Point3 start, int maxRowDistance, Action<LevelCell> del, Func<LevelCell, bool> ignoreCheck) {
            if (World.Get<MapSystem>().GetCell(start) == null) {
                return;
            }
            CheckRow(start, start, maxRowDistance, del, ignoreCheck, new[] { new Point3(0, 0, 1), new Point3(0, 0, -1) }, new Point3(-1,0,0));
            CheckRow(start, start, maxRowDistance, del, ignoreCheck, new[] { new Point3(0, 0, 1), new Point3(0, 0, -1) }, new Point3(1, 0, 0));
            CheckRow(start, start, maxRowDistance, del, ignoreCheck, new[] { new Point3(1, 0, 0), new Point3(-1, 0, 0) }, new Point3(0, 0, 1));
            CheckRow(start, start, maxRowDistance, del, ignoreCheck, new[] { new Point3(1, 0, 0), new Point3(-1, 0, 0) }, new Point3(0, 0, -1));
        }

        public static void CheckRow(Point3 origin, Point3 checkStart, int maxRowDistance, Action<LevelCell> del, Func<LevelCell, bool> ignoreCheck, Point3[] adjacent, Point3 increment) {
            var ms = World.Get<MapSystem>();
            var currCell = ms.GetCell(checkStart);
            if (currCell == null) {
                return;
            }
            del(currCell);
            for (int i = 1; i < maxRowDistance; i++) {
                var pos = checkStart + (increment * i);
                if (currCell.BlocksVision(currCell.WorldPosition.GetTravelDirTo(pos).ToDirectionEight())) {
                    continue;
                }
                currCell = ms.GetCell(pos);
                if (currCell == null || ignoreCheck(currCell)) {
                    break;
                }
                del(currCell);
                if (origin.Distance(pos) > maxRowDistance) {
                    break;
                }
                for (int a = 0; a < adjacent.Length; a++) {
                    var adjPos = pos + adjacent[a];
                    if (currCell.BlocksVision(pos.GetTravelDirTo(adjPos).ToDirectionEight())) {
                        continue;
                    }
                    CheckRow(origin, adjPos, maxRowDistance, del, ignoreCheck, new[]{ increment, increment * -1}, adjacent[a]);
                }
            }
        }

        public static void CheckRow(ref PointList list, Point3 origin, Point3 checkStart, int maxRowDistance, Point3[] adjacent, Point3 increment) {
            var ms = World.Get<MapSystem>();
            var currCell = ms.GetCell(checkStart);
            if (currCell == null) {
                return;
            }
            if (!list.Contains(checkStart)) {
                list.TryAdd(checkStart);
            }
            for (int i = 1; i < maxRowDistance; i++) {
                var pos = checkStart + (increment * i);
                if (currCell.BlocksVision(currCell.WorldPosition.GetTravelDirTo(pos).ToDirectionEight())) {
                    continue;
                }
                currCell = ms.GetCell(pos);
                if (currCell == null || list.Contains(pos)) {
                    break;
                }
                list.Add(pos);
                if (origin.Distance(pos) > maxRowDistance) {
                    break;
                }
                for (int a = 0; a < adjacent.Length; a++) {
                    var adjPos = pos + adjacent[a];
                    if (currCell.BlocksVision(pos.GetTravelDirTo(adjPos).ToDirectionEight())) {
                        continue;
                    }
                    CheckRow(ref list, origin, adjPos, maxRowDistance, new[] { increment, increment * -1 }, adjacent[a]);
                }
            }
        }
    }
}
