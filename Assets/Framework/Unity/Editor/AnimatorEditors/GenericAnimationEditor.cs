using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using XNodeEditor;

namespace PixelComrades {
    [CustomNodeGraphEditor(typeof(PoseAnimation))]
    public sealed class PoseAnimationEditor : NodeGraphEditor {
        //make a simple 0-1 normalized animation tester. Play inits and then show scroll bar
        public override void AddContextMenuItems(GenericMenu menu) {
            base.AddContextMenuItems(menu);
        }

        public override void OnGUI() {
            base.OnGUI();
        }

        public override string GetNodeMenuName(Type type) {
            if (typeof(BaseAnimationNode).IsAssignableFrom(type)) {
                return type.Name;
            }
            return null;
        }
    }

    [CustomNodeEditor(typeof(AnimationNode))]
    public class AnimationNodeEditor : NodeEditor {

        

    }

    [CustomEditor(typeof(GenericAnimation), true)]
    public class GenericAnimationEditor : Editor {

        public override void OnInspectorGUI() {
            var script = (GenericAnimation) target;
            if (GUILayout.Button("Open Editor")) {
                var window = CustomAnimationWindow.ShowWindow();
                window.Set(script);
            }
            base.OnInspectorGUI();
        }
    }
}
