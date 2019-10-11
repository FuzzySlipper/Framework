using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PixelComrades {
    public sealed class EventNode : StateGraphNode {

        public EventCondition LoopCondition = null;
        public string EventName;
        public bool Loop = false;
        
        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
            bool changed = false;
#if UNITY_EDITOR
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            var animationLabels = AnimationEvents.GetNames().ToArray();
            var index = System.Array.IndexOf(animationLabels, EventName);
            var newIndex = UnityEditor.EditorGUILayout.Popup("Event", index, animationLabels);
            if (newIndex >= 0) {
                EventName = animationLabels[newIndex];
                UnityEditor.EditorUtility.SetDirty(this);
            }
            if (!Loop) {
                if (GUILayout.Button("Add Loop Condition", buttonStyle)) {
                    Loop = true;
                    LoopCondition = new EventCondition();
                    changed = true;
                }
            }
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            if (Loop && LoopCondition != null) {
                LoopCondition.DrawGui(this, textStyle, buttonStyle);
            }
            if (changed) {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
            return changed;
        }

        private void RemoveCondition() {
            Loop = false;
        }

        public override string Title { get { return "Event: " + EventName; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }
        
        [System.Serializable]
        public class EventCondition : ConditionChecker{
            public void DrawGui(EventNode node, GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("While", textStyle);
                DrawComparison(textStyle, buttonStyle);
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                if (DrawType(node.Graph, textStyle, buttonStyle)) {
                    UnityEditor.EditorUtility.SetDirty(node);
                }
                if (GUILayout.Button("X", buttonStyle)) {
                    node.RemoveCondition();
                }
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
#endif
            }
        }

        public class RuntimeNode : RuntimeStateNode {

            private EventNode _originalNode;
            private RuntimeStateNode _exitNode;
            private RuntimeConditionChecker _loopCondition;
            
            public override RuntimeStateNode GetExitNode() {
                return _exitNode;
            }

            public RuntimeNode(EventNode node, RuntimeStateGraph graph) : base(node,graph) {
                var outNode = graph.OriginalGraph.GetConnectionEndpoint(node.OutPoints[0]);
                if (outNode != null) {
                    _exitNode = graph.GetRuntimeNode(outNode.Id);
                }
                _originalNode = node;
                if (_originalNode.Loop) {
                    _loopCondition = _originalNode.LoopCondition.GetRuntime();
                }
            }


            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                if (!_originalNode.Loop) {
                    Graph.Entity.Post(new AnimationEventTriggered(Graph.Entity, _originalNode.EventName));
                }
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt)) {
                    return true;
                }
                if (_loopCondition == null) {
                    return true;
                }
                if (_loopCondition.IsTrue(this)) {
                    Graph.Entity.Post(new AnimationEventTriggered(Graph.Entity, _originalNode.EventName));
                    return false;
                }
                return true;
            }
        }
    }
}
