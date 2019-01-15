using Sirenix.OdinInspector.Editor;

using UnityEditor;
using UnityEngine;

namespace PixelComrades {
    [CustomEditor(typeof(Tentacles), true), CanEditMultipleObjects] 
    public class TentaclesEditor : OdinEditor {

        public override void OnInspectorGUI() {
            
            if (GUILayout.Button("Build mesh")) {
                for (int i = 0; i < targets.Length; i++) {
                    var script = (Tentacles) targets[i];
                    script.BuildMesh();
                }
            }
            base.OnInspectorGUI();
        }
    }
}