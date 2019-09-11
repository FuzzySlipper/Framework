using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

namespace PixelComrades {
    public class Profile {
        private static Dictionary<string, ProfilePoint> _profiles = new Dictionary<string, ProfilePoint>();
        private static DateTime _startTime = DateTime.UtcNow;
        private static Stopwatch _stopWatch;
        private static TimeSpan _maxIdle = TimeSpan.FromSeconds(10);

        private Profile() {
        }

        public static DateTime UtcNow {
            get {
                if (_stopWatch == null || _startTime.Add(_maxIdle) < DateTime.UtcNow) {
                    _startTime = DateTime.UtcNow;
                    _stopWatch = Stopwatch.StartNew();
                }
                return _startTime.AddTicks(_stopWatch.Elapsed.Ticks);
            }
        }

        public struct ProfilePoint {
            public DateTime LastRecorded;
            public TimeSpan TotalTime;
            public int TotalCalls;
        }

        public static void EndProfile(string tag) {
            if (!_profiles.ContainsKey(tag)) {
                Debug.LogError("Can only end profiling for a tag which has already been started (tag was " + tag + ")");
                return;
            }
            ProfilePoint point = _profiles[tag];
            point.TotalTime += UtcNow - point.LastRecorded;
            ++point.TotalCalls;
            _profiles[tag] = point;
        }

        public static void PrintResults() {
            TimeSpan endTime = DateTime.UtcNow - _startTime;
            StringBuilder output = new StringBuilder();
            output.Append("============================\n\t\t\t\tProfile results:\n============================\n");
            foreach (KeyValuePair<string, ProfilePoint> pair in _profiles) {
                double totalTime = pair.Value.TotalTime.TotalSeconds;
                int totalCalls = pair.Value.TotalCalls;
                if (totalCalls < 1) {
                    continue;
                }
                output.Append("\nProfile ");
                output.Append(pair.Key);
                output.Append(" took ");
                output.Append(totalTime.ToString("F9"));
                output.Append(" seconds to complete over ");
                output.Append(totalCalls);
                output.Append(" iteration");
                if (totalCalls != 1) {
                    output.Append("s");
                }
                output.Append(", averaging ");
                output.Append((totalTime / totalCalls).ToString("F9"));
                output.Append(" seconds per call");
            }
            output.Append("\n\n============================\n\t\tTotal runtime: ");
            output.Append(endTime.TotalSeconds.ToString("F3"));
            output.Append(" seconds\n============================");
            Debug.Log(output.ToString());
        }

        public static void Reset() {
            _profiles.Clear();
            _startTime = DateTime.UtcNow;
        }

        public static void StartProfile(string tag) {
            ProfilePoint point;
            _profiles.TryGetValue(tag, out point);
            point.LastRecorded = UtcNow;
            _profiles[tag] = point;
        }
    }
}