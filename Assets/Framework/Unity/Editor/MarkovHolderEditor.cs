using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace PixelComrades {
    [CustomEditor(typeof(MarkovHolder))]
    public class MarkovHolderEditor : Editor {
        [SerializeField] private string _lastWord = "";

        public override void OnInspectorGUI() {
            var script = (MarkovHolder)target;
            if (GUILayout.Button("Reset")) {
                script.Reset();
            }
            if (GUILayout.Button("Random Word")) {
                _lastWord = script.GetName();
            }
            if (!string.IsNullOrEmpty(_lastWord)) {
                GUILayout.Label(string.Format("Generated: {0}", _lastWord));
            }
            base.OnInspectorGUI();
        }
    }
}
