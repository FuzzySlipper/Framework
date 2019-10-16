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
                    if (string.IsNullOrEmpty(trigger)) {
                        continue;
                    }
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
            if (GUILayout.Button("Fix Connection Points")) {
                for (int i = 0; i < script.Nodes.Count; i++) {
                    for (int l = 0; l < script.Nodes[i].InPoints.Count; l++) {
                        script.Nodes[i].InPoints[l].Owner = script.Nodes[i];
                        if (script.Nodes[i].InPoints[l].Id <= 0) {
                            script.Nodes[i].InPoints[l].Id = script.Nodes[i].FindMinConnectionId();
                        }
                    }
                    for (int l = 0; l < script.Nodes[i].OutPoints.Count; l++) {
                        script.Nodes[i].OutPoints[l].Owner = script.Nodes[i];
                        if (script.Nodes[i].OutPoints[l].Id <= 0) {
                            script.Nodes[i].OutPoints[l].Id = script.Nodes[i].FindMinConnectionId();
                        }
                    }
                }
            }
            
            base.OnInspectorGUI();
        }
    }
}