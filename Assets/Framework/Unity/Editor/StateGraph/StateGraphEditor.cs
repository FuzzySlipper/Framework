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
            if (GUILayout.Button("Add Default Triggers")) {
                var list = GraphTriggers.GetValues();
                for (int l = 0; l < list.Length; l++) {
                    var trigger = list[l];
                    bool foundTrigger = false;
                    for (int aIndex = 0; aIndex < script.GlobalTriggers.Count; aIndex++) {
                        if (script.GlobalTriggers[aIndex].Key == trigger) {
                            foundTrigger = true;
                            break;
                        }
                    }
                    if (!foundTrigger) {
                        script.GlobalTriggers.Add(new GraphTrigger(){ Key = trigger});
                    }
                }
            }
            
            base.OnInspectorGUI();
        }
    }
}