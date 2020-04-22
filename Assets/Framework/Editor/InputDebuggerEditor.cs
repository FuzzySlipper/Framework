using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace PixelComrades {
    [CustomEditor(typeof(InputDebugger), true)]
    public class InputDebuggerEditor : OdinEditor {
    public override void OnInspectorGUI() {
            var script = (InputDebugger) target;
            if (!Game.GameActive || !Application.isPlaying) {
                EditorGUILayout.LabelField("Game Not Active");
                return;
            }
            EditorGUILayout.LabelField(string.Format("Time: {0}", TimeManager.Time.ToString("F4")));
            EditorGUILayout.LabelField(string.Format("Look: {0}", PlayerInputSystem.LookInput));
            EditorGUILayout.LabelField(string.Format("Move: {0}", PlayerInputSystem.MoveInput));
            EditorGUILayout.LabelField(string.Format("IsCursorOverUI: {0}", PlayerInputSystem.IsCursorOverUI));
            if (script.CheckButtons != null) {
                for (int i = 0; i < script.CheckButtons.Length; i++) {
                    var button = script.CheckButtons[i];
                    if (string.IsNullOrEmpty(button)) {
                        continue;
                    }
                    EditorGUILayout.LabelField(string.Format("{0} is down: {1}", button, PlayerInputSystem.GetButton(button)));
                }
            }
            //if (GUILayout.Button("Test")) {}
            base.OnInspectorGUI();
        }
    }
}