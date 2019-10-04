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
                for (int i = 0; i < TimeManager.ActiveCount; i++) {
                    if (i > MaxList) {
                        break;
                    }
                    var current = script[i];
                    if (current== null) {
                        continue;
                    }
                    var taskDisplay = string.Format("{0}: {1} Scale {2} Curr {3} Wait {4}",
                        current.Routine.ToString(), current.Mode, !current.Unscaled, current.Current.ToString("F2"), current.WaitFor);
                    if (GUILayout.Button(taskDisplay)) {
                        script.CancelInternal(current);
                    }
                }
            }
            if (GUILayout.Button("Cancel All")) {
                script.CancelAll();
            }
            if (GUILayout.Button("Check Editor")) {
                script.ForceEditorCheck();
                Debug.Log(TimeManager.ActiveCount + " " + EditorApplication.update.GetInvocationList().Length);
                foreach (var del in EditorApplication.update.GetInvocationList()) {
                    if (del.Target == null) {
                        continue;
                    }
                    Debug.Log(del.Target.GetType());
                }
            }
            if (GUILayout.Button("Test")) {
                TimeManager.StartTask(Test3(1.5f));
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
                TimeManager.StartTask(Test(2)), TimeManager.StartTask(Test(4))
            };
            Log(extraWait, 3);
        }

        private void Log(float identifier, int stage) {
            Debug.Log(string.Format("{0} {1}: {2}", identifier, stage, Time.realtimeSinceStartup));
        }
    }
}