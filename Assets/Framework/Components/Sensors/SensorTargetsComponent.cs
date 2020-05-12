using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Priority(Priority.Normal)]
    [System.Serializable]
	public sealed class SensorTargetsComponent : IComponent {
        
        public int MaxUpdatesNoContact = 150;
        public List<WatchTarget> WatchTargets = new List<WatchTarget>();
        public int Faction;
        public bool Shouted = false;

        public SensorTargetsComponent(int faction) {
            Faction = faction;
        }

        public SensorTargetsComponent(SerializationInfo info, StreamingContext context) {
            MaxUpdatesNoContact = info.GetValue(nameof(MaxUpdatesNoContact), MaxUpdatesNoContact);
            WatchTargets = info.GetValue(nameof(WatchTargets), WatchTargets);
            Faction = info.GetValue(nameof(Faction), Faction);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(MaxUpdatesNoContact), MaxUpdatesNoContact);
            info.AddValue(nameof(WatchTargets), WatchTargets);
            info.AddValue(nameof(Faction), Faction);
        }
        
        public void AddWatch(Entity entity, bool isVision) {
            if (entity == null) {
                return;
            }
            entity = entity.GetRoot();
            if (entity.Get<FactionComponent>().Value == Faction) {
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

        private void RemoveWatchTarget(int index) {
            WatchTargets.RemoveAt(index);
        }

        public void UpdateWatchTargets() {
            for (int i = WatchTargets.Count - 1; i >= 0; i--) {
                if (WatchTargets[i].Target == null || WatchTargets[i].Target.IsDead) {
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

    }
}
