using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SensorSimpleComponent : IComponent, IReceive<DamageEvent> {
        
        public int MaxUpdatesNoContact = 50;
        public List<WatchTarget> WatchTargets = new List<WatchTarget>();
        public int Owner { get; set; }
        public int Faction;

        public SensorSimpleComponent(int faction) {
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

        public void UpdateWatchTargets() {
            for (int i = WatchTargets.Count - 1; i >= 0; i--) {
                if (WatchTargets[i].Target == null || WatchTargets[i].Target.Stats.HealthStat?.Current < 0) {
                    RemoveWatch(WatchTargets[i]);
                    continue;
                }
                WatchTargets[i].LastSensedTurnCount++;
                if (WatchTargets[i].LastSensedTurnCount > MaxUpdatesNoContact) {
                    RemoveWatch(WatchTargets[i]);
                }
            }
            WatchTargets.Sort(this.GetEntity().Get<GridPosition>());
        }

        public void Handle(DamageEvent arg) {
            AddWatch(arg.Origin, true);
        }
    }
}
