using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SensorSimpleComponent : IComponent {
        
        private int _maxTurnsToRememberSeenUnit = 5;
        private int _maxTurnsToRememberHeardUnit = 3;

        public int MaxHearDistance { get; private set; }
        public int MaxVisionDistance { get; private set; }
        public int MaxTurnsNpcVisible = 12;
        public List<WatchTarget> WatchTargets = new List<WatchTarget>();
        public List<Entity> TempList = new List<Entity>();
        public int Owner { get; set; }
        public int Faction;

        public SensorSimpleComponent(int faction, int maxHearDistance = 12, int maxVisionDistance = 5) {
            MaxHearDistance = maxHearDistance;
            MaxVisionDistance = maxVisionDistance;
            Faction = faction;
        }
        
        public void AddWatch(Entity entity, bool isVision) {
            if (entity == null) {
                return;
            }
            var watcher = GetWatchTarget(entity);
            if (watcher == null) {
                watcher = new WatchTarget();
                WatchTargets.Add(watcher);
                watcher.Target = entity;
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

        private void RemoveWatchTarget(int index) {
            WatchTargets.RemoveAt(index);
        }

        //private Mesh _coneMesh;
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
            for (int i = WatchTargets.Count - 1; i >= 0; i--) {
                var watchTarget = WatchTargets[i];
                if (watchTarget.Target == null || watchTarget.Target.Stats.HealthStat?.Current < 0) {
                    RemoveWatchTarget(i);
                    continue;
                }
                watchTarget.LastSensedTurnCount++;
                if (watchTarget.Seen && watchTarget.LastSensedTurnCount > _maxTurnsToRememberSeenUnit) {
                    RemoveWatchTarget(i);
                }
                else if (!watchTarget.Seen && watchTarget.LastSensedTurnCount > _maxTurnsToRememberHeardUnit) {
                    RemoveWatchTarget(i);
                }
            }
            WatchTargets.Sort(this.GetEntity().Get<GridPosition>());
        }
    }
}
