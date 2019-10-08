using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace PixelComrades {
    [CustomEditor(typeof(GenericSequence), true)]
    public class GenericSequenceEditor : Editor {

        public override void OnInspectorGUI() {
            var script = (GenericSequence) target;
            if (GUILayout.Button("Open Editor")) {
                var window = SequencerWindow.ShowWindow();
                window.Set(script);
            }
            base.OnInspectorGUI();
        }
    }
}
