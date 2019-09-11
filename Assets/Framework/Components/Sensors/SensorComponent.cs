using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class SensorComponent : IComponent {

        public List<WatchTarget> WatchTargets = new List<WatchTarget>();
        public PointList DetectedCells = new PointList();

        public Point3 LastDetectedCenter;
        public int MaxHearDistance { get; private set; }
        public int MaxVisionDistance { get; private set; }

        public SensorComponent(int maxHearDistance = 12, int maxVisionDistance = 6) {
            MaxHearDistance = maxHearDistance;
            MaxVisionDistance = maxVisionDistance;
        }

        public SensorComponent(SerializationInfo info, StreamingContext context) {
            LastDetectedCenter = info.GetValue(nameof(LastDetectedCenter), LastDetectedCenter);
            MaxHearDistance = info.GetValue(nameof(MaxHearDistance), MaxHearDistance);
            MaxVisionDistance = info.GetValue(nameof(MaxVisionDistance), MaxVisionDistance);
            DetectedCells = info.GetValue(nameof(DetectedCells), DetectedCells);
            WatchTargets = info.GetValue(nameof(WatchTargets), WatchTargets);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(LastDetectedCenter), LastDetectedCenter);
            info.AddValue(nameof(MaxHearDistance), MaxHearDistance);
            info.AddValue(nameof(MaxVisionDistance), MaxVisionDistance);
            info.AddValue(nameof(DetectedCells), DetectedCells);
            info.AddValue(nameof(WatchTargets), WatchTargets);
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
    }
}
