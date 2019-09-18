using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {
    [Priority(Priority.Higher)]
    public class SensorSystem : SystemBase, IPeriodicUpdate, IReceive<DamageEvent> {

        public GameOptions.CachedInt MaxTurnsNpcVisible = new GameOptions.CachedInt("MaxTurnsNpcVisible");
        public bool CheckVision = false;

        private List<SensorDetectingNode> _sensorNodes;
        private List<UnitySensorNode> _unitySensorNodes;
        private List<UnitOccupyingCellNode> _occupyNodes;
        private List<Entity> _tempEnemyList = new List<Entity>();

        private const float HearingSectorSize = 25f;
        private const int HearingChance = 10;

        public SensorSystem() {
            NodeFilter<SensorDetectingNode>.New(SensorDetectingNode.GetTypes());
            NodeFilter<UnitySensorNode>.New(UnitySensorNode.GetTypes());
            EntityController.RegisterReceiver<SensorTargetsComponent>(this);
        }

        public override void Dispose() {
            base.Dispose();
            if (_occupyNodes != null) {
                _occupyNodes.Clear();
            }
        }

        public void OnPeriodicUpdate() {
            if (_occupyNodes == null) {
                _occupyNodes = EntityController.GetNodeList<UnitOccupyingCellNode>();
            }
            if (_sensorNodes == null) {
                _sensorNodes = EntityController.GetNodeList<SensorDetectingNode>();
            }
            if (_unitySensorNodes == null) {
                _unitySensorNodes = EntityController.GetNodeList<UnitySensorNode>();
            }
            CheckNodeList();
            CheckUnitySensors();
        }

        private void CheckUnitySensors() {
            if (_unitySensorNodes == null || !Game.GameActive || Game.Paused) {
                return;
            }
            for (int n = 0; n < _unitySensorNodes.Count; n++) {
                var node = _unitySensorNodes[n];
                node.Sensor.Sensor.Pulse();
                node.Targets.UpdateWatchTargets();
                for (int i = 0; i < node.Sensor.Sensor.DetectedColliders.Count; i++) {
                    node.Targets.AddWatch(UnityToEntityBridge.GetEntity(node.Sensor.Sensor.DetectedColliders[i]), true);
                }
                if (node.Targets.WatchTargets.Count == 0) {
                    _tempEnemyList.Clear();
                    World.Get<FactionSystem>().FillFactionEnemiesList(_tempEnemyList, node.Faction.Faction);
                    var nodePos = node.Tr.position.WorldToGenericGrid(HearingSectorSize);
                    for (int f = 0; f < _tempEnemyList.Count; f++) {
                        var enemy = _tempEnemyList[f];
                        var tr = enemy.Get<TransformComponent>()?.Value;
                        if (tr == null) {
                            Debug.Log("Enemy has no TR " + enemy.DebugId);
                            continue;
                        }
                        if (tr.position.WorldToGenericGrid(HearingSectorSize) == nodePos) {
                            var hearingChance = HearingChance;
                            if (enemy.Tags.Contain(EntityTags.PerformingCommand)) {
                                hearingChance *= 4;
                            }
                            else if (enemy.Tags.Contain(EntityTags.Moving)) {
                                hearingChance *= 2;
                            }
                            if (Game.Random.DiceRollSucess(hearingChance)) {
                                if (!UnityEngine.Physics.Linecast(tr.position, node.Tr.position, LayerMasks.Walls)) {
                                    node.Targets.AddWatch(enemy, false);
                                    DebugText.Log(node.Entity.DebugId + " spotted " + enemy.DebugId);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CheckNodeList() {
            if (_occupyNodes == null || _sensorNodes == null || !Game.GameActive || Game.Paused ) {
                return;
            }
            var ls = World.Get<LineOfSightSystem>();
            for (int n = 0; n < _sensorNodes.Count; n++) {
                var node = _sensorNodes[n];
                var sensor = node.Sensor;
                sensor.DetectedCells.Clear();
                var start = node.Position.Position;
                sensor.LastDetectedCenter = start;
                var fwd = node.Tr.ForwardDirection2D();
                for (int i = 0; i < DirectionsExtensions.Length2D; i++) {
                    var dir = (Directions) i;
                    var maxRowDistance = dir == fwd ? sensor.MaxVisionDistance : sensor.MaxHearDistance;
                    var adjacent = dir.Adjacent();
                    ShadowFloodFill.CheckRow(ref sensor.DetectedCells,start, start, 
                        maxRowDistance, new[] { adjacent[0].ToPoint3(), adjacent[1].ToPoint3()}, dir.ToPoint3());
                }
                for (int i = 0; i < _occupyNodes.Count; i++) {
                    var visible = _occupyNodes[i];
                    if (visible.Entity == node.Entity) {
                        continue;
                    }
                    if (!sensor.DetectedCells.Contains(visible.Position)) {
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

        public void Handle(DamageEvent arg) {
            var sensorTargets = arg.Target.Entity.Find<SensorTargetsComponent>();
#if DEBUG
            DebugLog.Add(
                sensorTargets.GetEntity().DebugId + " was attacked by " + arg.Origin?.Entity.DebugId + " parent " +
                arg.Origin?.Entity.ParentId + " is pooled " + arg.Origin?.Entity.Pooled);
#endif
            sensorTargets.AddWatch(arg.Origin, true);
        }
    }
}
