using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class GlobalConditionNode : ConditionNode {
        public override bool IsGlobal { get => true; }
        public override int InputMin { get => 0; }
        public override int InputMax { get => 0; }

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
            bool changed = false;
#if UNITY_EDITOR
            for (int i = 0; i < Conditions.Count; i++) {
                Conditions[i].DrawGui(this, textStyle, buttonStyle);
                GUILayout.Space(10);
            }
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button("Add Condition", buttonStyle)) {
                Conditions.Add(new ConditionExit());
                CheckSize();
                changed = true;
            }
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            if (changed) {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
            return changed;
        }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new GlobalConditionRuntimeNode(this, graph);
        }

        public class GlobalConditionRuntimeNode : RuntimeConditionNode, IGlobalRuntimeStateNode {
            public GlobalConditionRuntimeNode(GlobalConditionNode node, RuntimeStateGraph graph) : base(node, graph) {}

            public void CheckConditions() {
                if (HasTrueCondition()) {
                    var exitNode = GetConditionExitNode();
                    if (exitNode != null) {
                        Graph.ChangeNode(exitNode);
                    }
                }
            }

            public override bool TryComplete(float dt) {
                return true;
            }
        }
    }
}
