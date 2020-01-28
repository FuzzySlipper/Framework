using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PixelComrades {
    [Priority(Priority.Higher)]
    public class SensorSystem : SystemBase, IPeriodicUpdate, IReceive<ReceivedDamageEvent> {

        public GameOptions.CachedInt MaxTurnsNpcVisible = new GameOptions.CachedInt("MaxTurnsNpcVisible");
        public bool CheckVision = false;

        private TemplateList<SensorDetectingTemplate> _sensorTemplates;
        private TemplateList<UnitySensorTemplate> _unitySensorTemplates;
        private TemplateList<UnitOccupyingCellTemplate> _occupyTemplates;
        
        private ManagedArray<SensorDetectingTemplate>.RefDelegate _sensorDel;
        private ManagedArray<UnitySensorTemplate>.RefDelegate _unitySensorDel;
        
        private List<Entity> _tempEnemyList = new List<Entity>();

        private const float HearingSectorSize = 25f;
        private const int HearingChance = 10;

        public SensorSystem() {
            _sensorDel = RunUpdate;
            _unitySensorDel = RunUpdate;
            TemplateFilter<SensorDetectingTemplate>.Setup();
            _sensorTemplates = EntityController.GetTemplateList<SensorDetectingTemplate>();
            TemplateFilter<UnitySensorTemplate>.Setup();
            _unitySensorTemplates = EntityController.GetTemplateList<UnitySensorTemplate>();
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(SensorTargetsComponent)
            }));
        }

        public override void Dispose() {
            base.Dispose();
            if (_occupyTemplates != null) {
                _occupyTemplates.Clear();
            }
        }

        public void OnPeriodicUpdate() {
            if (_occupyTemplates == null) {
                _occupyTemplates = EntityController.GetTemplateList<UnitOccupyingCellTemplate>();
            }
            if (Game.GameActive && !Game.Paused) {
                _sensorTemplates.Run(_sensorDel);
                _unitySensorTemplates.Run(_unitySensorDel);
            }
        }

        private void RunUpdate(ref UnitySensorTemplate template) {
            template.Sensor.Sensor.Pulse();
            template.Targets.UpdateWatchTargets();
            for (int i = 0; i < template.Sensor.Sensor.DetectedColliders.Count; i++) {
                var enemy = UnityToEntityBridge.GetEntity(template.Sensor.Sensor.DetectedColliders[i]);
                if (enemy == null || enemy == template.Entity) {
                    continue;
                }
                var faction = enemy.Get<FactionComponent>();
                if (!World.Get<FactionSystem>().AreEnemies(faction, template.Faction)) {
                    continue;
                }
                template.Targets.AddWatch(enemy, true);
                //Console.Log(template.Entity.DebugId + " saw " + enemy.DebugId);
            }
            if (template.Targets.WatchTargets.Count != 0) {
                return;
            }
            _tempEnemyList.Clear();
            World.Get<FactionSystem>().FillFactionEnemiesList(_tempEnemyList, template.Faction.Faction);
            var nodePos = template.Tr.position.WorldToGenericGrid(HearingSectorSize);
            for (int f = 0; f < _tempEnemyList.Count; f++) {
                var enemy = _tempEnemyList[f];
                var tr = enemy.Get<TransformComponent>();
                if (tr == null) {
                    Debug.Log("Enemy has no TR " + enemy.DebugId);
                    continue;
                }
                if (tr.position.WorldToGenericGrid(HearingSectorSize) != nodePos) {
                    continue;
                }
                var hearingChance = HearingChance;
//                if (enemy.AnimGraph.Value.CurrentTag != GraphNodeTags.Action) {
//                    hearingChance *= 4;
//                }
                if (enemy.Tags.Contain(EntityTags.Moving)) {
                    hearingChance *= 2;
                }
                if (Game.Random.DiceRollSucess(hearingChance)) {
                    if (!Physics.Linecast(tr.position, template.Tr.position, LayerMasks.Walls)) {
                        template.Targets.AddWatch(enemy, false);
                        Console.Log(template.Entity.DebugId + " heard " + enemy.DebugId);
                    }
                }
            }
        }

        private void RunUpdate(ref SensorDetectingTemplate template) {
            var sensor = template.Sensor;
            sensor.DetectedCells.Clear();
            var start = template.Position.Position;
            sensor.LastDetectedCenter = start;
            var fwd = template.Tr.ForwardDirection2D();
            var ls = World.Get<LineOfSightSystem>();
            for (int i = 0; i < DirectionsExtensions.Length2D; i++) {
                var dir = (Directions) i;
                var maxRowDistance = dir == fwd ? sensor.MaxVisionDistance : sensor.MaxHearDistance;
                var adjacent = dir.Adjacent();
                ShadowFloodFill.CheckRow(
                    ref sensor.DetectedCells, start, start,
                    maxRowDistance, new[] {adjacent[0].ToPoint3(), adjacent[1].ToPoint3()}, dir.ToPoint3());
            }
            for (int i = 0; i < _occupyTemplates.Max; i++) {
                if (_occupyTemplates.IsInvalid(i)) {
                    continue;
                }
                var visible = _occupyTemplates[i];
                if (visible.Entity == template.Entity) {
                    continue;
                }
                if (!sensor.DetectedCells.Contains(visible.Position)) {
                    continue;
                }
                var isVision = true;
                if (CheckVision) {
                    ls.CanSeeOrHear(template.Entity, visible.Entity, out isVision);
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

        private Collider[] _shoutColliders = new Collider[40];
        private const float ShoutRadius = 30f;
        
        public void Handle(ReceivedDamageEvent arg) {
            var sensorTargets = arg.Target.Entity.Find<SensorTargetsComponent>();
//#if DEBUG
//            DebugLog.Add(
//                sensorTargets.GetEntity().DebugId + " was attacked by " + arg.Origin?.Entity.DebugId + " parent " +
//                arg.Origin?.Entity.ParentId + " is pooled " + arg.Origin?.Entity.Pooled);
//#endif
            sensorTargets.AddWatch(arg.Origin, true);
            if (sensorTargets.Shouted) {
                return;
            }
            var sourceFaction = arg.Target.Faction.Faction;
            var factionSystem = World.Get<FactionSystem>();
            sensorTargets.Shouted = true;
            var sourcePos = arg.Target.Tr.position;
            var limit = Physics.OverlapSphereNonAlloc(sourcePos, ShoutRadius, _shoutColliders, LayerMasks.DefaultCollision);
            for (int i = 0; i < limit; i++) {
                var sensorTemplate = UnityToEntityBridge.GetEntity(_shoutColliders[i]).GetTemplate<UnitySensorTemplate>();
                if (sensorTemplate == null) {
                    continue;
                }
                if (!factionSystem.AreFriends(sourceFaction, sensorTemplate.Faction.Faction)) {
                    continue;
                }
                if (Physics.Linecast(sourcePos, sensorTemplate.Tr.position, LayerMasks.Walls)) {
                    continue;
                }
                sensorTemplate.Targets.AddWatch(arg.Origin, false);
            }
        }
    }
}
