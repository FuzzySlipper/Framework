﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class SensorCellsComponent : IComponent {
        public System.Action OnUpdate;
        public List<WatchTarget> WatchTargets = new List<WatchTarget>();
        public int MaxHearDistance { get; private set; }
        public int MaxVisionDistance { get; private set; }
        public int MaxTurnsNpcVisible = 12;

        public BufferedList<LevelCell> Cells = new BufferedList<LevelCell>();

        public SensorCellsComponent(int maxHearDistance = 12, int maxVisionDistance = 5) {
            MaxHearDistance = maxHearDistance;
            MaxVisionDistance = maxVisionDistance;
        }

        public SensorCellsComponent(SerializationInfo info, StreamingContext context) {
            WatchTargets = info.GetValue(nameof(WatchTargets), WatchTargets);
            MaxHearDistance = info.GetValue(nameof(MaxHearDistance), MaxHearDistance);
            MaxVisionDistance = info.GetValue(nameof(MaxVisionDistance), MaxVisionDistance);
            MaxTurnsNpcVisible = info.GetValue(nameof(MaxTurnsNpcVisible), MaxTurnsNpcVisible);
            Cells = info.GetValue(nameof(Cells), Cells);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(WatchTargets), WatchTargets);
            info.AddValue(nameof(MaxHearDistance), MaxHearDistance);
            info.AddValue(nameof(MaxVisionDistance), MaxVisionDistance);
            info.AddValue(nameof(MaxTurnsNpcVisible), MaxTurnsNpcVisible);
            info.AddValue(nameof(Cells), Cells);
        }
        
        public void UpdateSenses() {
            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();
            //ShadowFloodFill.GetVisiblePoints(Tr.position.ToCellGridP3(), MaxHearDistance, UpdateCellMapVisible, CurrentList.Contains);
            //watch.Stop();
            //Debug.LogFormat("Found {0} in {1}" ,CurrentList.Count, watch.Elapsed.TotalMilliseconds);
            var owner = this.GetEntity();
            var start = owner.Get<GridPosition>().Value;
            var fwd = owner.Get<TransformComponent>().ForwardDirection2D();
            for (int i = 0; i < DirectionsExtensions.Length2D; i++) {
                var dir = (Directions) i;
                var maxRowDistance = dir == fwd ? MaxVisionDistance : MaxHearDistance;
                var adjacent = dir.Adjacent();
                ShadowFloodFill.CheckRow(
                    start, start, maxRowDistance, UpdateCellMapVisible, Cells.Contains, new[] {
                        adjacent[0].ToPoint3(), adjacent[1].ToPoint3()
                    }, dir.ToPoint3());
            }
            UpdateWatchTargets();
            OnUpdate?.Invoke();
        }
        public void AddWatch(Entity entity, bool isVision) {
            if (entity == null) {
                return;
            }
            var watcher = GetWatchTarget(entity);
            if (watcher == null) {
                watcher = new WatchTarget(entity);
                WatchTargets.Add(watcher);
            }
            watcher.LastSensedTurnCount = 0;
            watcher.LastSensedPos = entity.Get<GridPosition>();
            if (watcher.Seen && !isVision) {
                return;
            }
            watcher.Seen = isVision;
        }

        public void RemoveWatch(WatchTarget watcher) {
            WatchTargets.Remove(watcher);
        }

        public void RemoveWatch(Entity e) {
            var watchTarget = GetWatchTarget(e);
            if (watchTarget != null) {
                RemoveWatch(watchTarget);
            }
        }

        public WatchTarget GetWatchTarget(Entity e) {
            for (int i = 0; i < WatchTargets.Count; i++) {
                if (WatchTargets[i].Target == e) {
                    return WatchTargets[i];
                }
            }
            return null;
        }

        public void UpdateWatchTargets() {
            for (int i = WatchTargets.Count - 1; i >= 0; i--) {
                if (WatchTargets[i].Target == null) {
                    RemoveWatch(WatchTargets[i]);
                    continue;
                }
                var cell = World.Get<MapSystem>().GetCell(WatchTargets[i].LastSensedPos);
                if (cell == null) {
                    continue;
                }
                if (cell.IsVisible) {
                    WatchTargets[i].Seen = true;
                    WatchTargets[i].LastSensedTurnCount = 0;
                }
                else {
                    WatchTargets[i].LastSensedTurnCount++;
                    if (WatchTargets[i].LastSensedTurnCount > MaxTurnsNpcVisible) {
                        RemoveWatch(WatchTargets[i]);
                    }
                }
            }
        }

        public void UpdateCellMapVisible(LevelCell cell) {
            if (Cells.Contains(cell)) {
                return;
            }
            //if (cell.HasActor()) {
            //    for (int i = 0; i < cell.Actors.Count; i++) {
            //        var actor = cell.Actors[i];
            //        if (actor != null) {
            //            AddWatch(actor, true);
            //        }
            //    }
            //}
            cell.IsVisible = true;
            Cells.Add(cell);
        }
    }
}
