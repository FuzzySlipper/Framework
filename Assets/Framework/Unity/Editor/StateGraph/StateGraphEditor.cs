using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace PixelComrades {
    [CustomEditor(typeof(StateGraph), true)]
    public class StateGraphEditor : OdinEditor {
    public override void OnInspectorGUI() {
            var script = (StateGraph) target;
            if (GUILayout.Button("Open Editor")) {
                var window = StateGraphWindow.ShowWindow();
                window.Set(script);
            }
            base.OnInspectorGUI();
        }
    }
}