using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Higher)]
    public class SensorSystem : SystemBase, IPeriodicUpdate {

        public GameOptions.CachedInt MaxTurnsNpcVisible = new GameOptions.CachedInt("MaxTurnsNpcVisible");
        public bool CheckVision = false;

        private List<SensorDetectingNode> _sensorList;
        private List<UnitOccupyingCellNode> _visibleNodes;

        public SensorSystem() {
            NodeFilter<SensorDetectingNode>.New(SensorDetectingNode.GetTypes());
        }

        public override void Dispose() {
            base.Dispose();
            _visibleNodes.Clear();
        }

        public void OnPeriodicUpdate() {
            if (_visibleNodes == null) {
                _visibleNodes = EntityController.GetNodeList<UnitOccupyingCellNode>();
            }
            if (_sensorList == null) {
                _sensorList = EntityController.GetNodeList<SensorDetectingNode>();
            }
            if (_visibleNodes == null || _sensorList == null || !Game.GameActive || Game.Paused ) {
                return;
            }
            var ls = World.Get<LineOfSightSystem>();
            for (int n = 0; n < _sensorList.Count; n++) {
                var node = _sensorList[n];
                var sensor = node.Sensor.c;
                sensor.DetectedCells.Clear();
                var start = node.Position.c.Position;
                sensor.LastDetectedCenter = start;
                var fwd = node.Entity.Tr.ForwardDirection2D();
                for (int i = 0; i < DirectionsExtensions.Length2D; i++) {
                    var dir = (Directions) i;
                    var maxRowDistance = dir == fwd ? sensor.MaxVisionDistance : sensor.MaxHearDistance;
                    var adjacent = dir.Adjacent();
                    ShadowFloodFill.CheckRow(ref sensor.DetectedCells,
                        start, start, maxRowDistance, new[] { adjacent[0].ToPoint3(), adjacent[1].ToPoint3()}, dir.ToPoint3());
                }
                for (int i = 0; i < _visibleNodes.Count; i++) {
                    var visible = _visibleNodes[i];
                    if (visible.Entity == node.Entity) {
                        continue;
                    }
                    if (!sensor.DetectedCells.Contains(visible.Position.c)) {
                        continue;
                    }
                    var isVision = true;
                    if (CheckVision) {
                        ls.CanSeeOrHear(node.Entity, visible.Entity, out isVision);
                    }
                    sensor.AddWatch(visible.Entity, isVision);
                }
                for (int w = sensor.WatchTargets.Count - 1; w >= 0; w--) {
                    if (sensor.WatchTargets[w].Target == null) {
                        sensor.RemoveWatch(sensor.WatchTargets[w]);
                        continue;
                    }
                    sensor.WatchTargets[w].LastSensedTurnCount++;
                    if (sensor.WatchTargets[w].LastSensedTurnCount > MaxTurnsNpcVisible) {
                        sensor.RemoveWatch(sensor.WatchTargets[w]);
                    }
                }
            }
        }
        
        //private void UpdateSenses(SensorSimpleComponent simple) {
        //    simple.TempList.Clear();
        //    World.Get<FactionSystem>().FillFactionEnemiesList(simple.TempList, simple.Faction);
        //    var owner = simple.GetEntity();
        //    var gridPos = owner.Get<GridPosition>().Position;
        //    for (int i = 0; i < simple.TempList.Count; i++) {
        //        var target = simple.TempList[i];
        //        if (gridPos.Distance(target.Get<GridPosition>().Position) > simple.MaxHearDistance) {
        //            continue;
        //        }
        //        if (World.Get<LineOfSightSystem>().CanSeeOrHear(owner, target, out var hearingBlocked)) {
        //            simple.AddWatch(target, true);
        //        }
        //        else if (!hearingBlocked) {
        //            simple.AddWatch(target, false);
        //        }
        //    }
        //    simple.UpdateWatchTargets();
        //}
    }
}
