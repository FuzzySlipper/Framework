using PixelComrades;
using UnityEditor;
using UnityEngine;

namespace PixelComrades {
    [CustomEditor(typeof(SetSortingLayer)), CanEditMultipleObjects]
    public class SetSortingLayerEditor : Editor {

        public override void OnInspectorGUI() {
            if (GUILayout.Button("Set")) {
                for (int i = 0; i < targets.Length; i++) {
                    var script = (SetSortingLayer) targets[i];
                    if (script != null) {
                        script.SetSortLayer();
                    }
                }
            }
            base.OnInspectorGUI();
        }
    }
}

