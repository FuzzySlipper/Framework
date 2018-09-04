using UnityEngine;
using System.Collections;
using UnityEditor;

namespace PixelComrades {
    [CustomEditor(typeof(TimeManager))] public class TimeManagerEditor : Editor {
        private const int MaxList = 30;

        private bool _expandedList = true;

        public override void OnInspectorGUI() {
            var script = (TimeManager) target;
            _expandedList = EditorGUILayout.Foldout(_expandedList, "Tasks");
            if (_expandedList) {
                for (int i = 0; i < script.ActiveCount; i++) {
                    if (i > MaxList) {
                        break;
                    }
                    var current = script[i];
                    if (current== null) {
                        continue;
                    }
                    var taskDisplay = string.Format("{0}: {1} Scale {2} Curr {3} Wait {4}",
                        current.Id, current.Mode, !current.Unscaled, current.Current.ToString("F2"), current.WaitFor);
                    if (GUILayout.Button(taskDisplay)) {
                        script.CancelInternal(current);
                    }
                }
            }
            if (GUILayout.Button("Cancel All")) {
                script.CancelAll();
            }
            if (GUILayout.Button("Check Editor")) {
                script.CheckEditor();
                Debug.Log(script.ActiveCount);
            }
            if (GUILayout.Button("Test")) {
                TimeManager.Start(Test3(1.5f));
            }
            if (GUILayout.Button("Cancel Correct")) {
                TimeManager.Cancel(Test3(1.5f).ToString());
            }
            if (GUILayout.Button("Cancel Different Time")) {
                TimeManager.Cancel(Test3(2f).ToString());
            }
            if (GUILayout.Button("Cancel Different Routine")) {
                TimeManager.Cancel(Test(15f).ToString());
            }
            EditorGUILayout.LabelField(string.Format("Scale {0} Fixed {1} Time {2} TimeUn {3}", 
                TimeManager.TimeScale, TimeManager.FixedDelta, TimeManager.Time, TimeManager.TimeUnscaled));
            base.OnInspectorGUI();
        }

        private IEnumerator Test(float extraWait) {
            yield return 1;
            yield return extraWait;
        }

        private IEnumerator Test3(float extraWait) {
            yield return 1;
            yield return extraWait;
            yield return new Task[] {
                TimeManager.Start(Test(2)), TimeManager.Start(Test(4))
            };
            Log(extraWait, 3);
        }

        private void Log(float identifier, int stage) {
            Debug.Log(string.Format("{0} {1}: {2}", identifier, stage, Time.realtimeSinceStartup));
        }
    }
}