using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PixelComrades {
    public class ConditionNode : StateGraphNode {
        
        private static Vector2 _defaultSize = new Vector2(225, 100);
        
        public List<Config> Conditions = new List<Config>();
        public int DefaultExit = 0;
        protected override Vector2 GetNodeSize { get { return _defaultSize; } }
        public override int OutputMax { get { return MaxConnections; } }

        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
            bool changed = false;
#if UNITY_EDITOR
            for (int i = 0; i < Conditions.Count; i++) {
                Conditions[i].DrawGui(this, textStyle, buttonStyle);
                GUILayout.Space(10);
            }
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("Else exit ", textStyle);
            var indices = new string[OutPoints.Count];
            for (int i = 0; i < indices.Length; i++) {
                indices[i] = i.ToString();
            }
            DefaultExit = UnityEditor.EditorGUILayout.Popup(DefaultExit, indices, textStyle);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button("Add Condition", buttonStyle)) {
                Conditions.Add(new Config());
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

        private void Remove(Config config) {
            Conditions.Remove(config);
            CheckSize();
        }

        protected void CheckSize() {
            Rect.size = new Vector2(_defaultSize.x, _defaultSize.y + ((_defaultSize.y * 0.75f) * Conditions.Count));
        }
        
        public override string Title { get { return "Condition"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeConditionNode(this, graph);
        }

        public class Config {
            public ConditionType Type;
            public ComparisonType Comparison;
            public string Value;
            public int Output;

            public void DrawGui(ConditionNode node, GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("If", textStyle);
                Type = (ConditionType) UnityEditor.EditorGUILayout.EnumPopup(Type, textStyle);
                Comparison = (ComparisonType) UnityEditor.EditorGUILayout.EnumPopup(Comparison, textStyle);
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                switch (Type) {
                    case ConditionType.Trigger:
                        var labels = node.Graph.Triggers.Select(t => t.Key).ToArray();
                        var index = System.Array.IndexOf(labels, Value);
                        var newIndex = UnityEditor.EditorGUILayout.Popup(index, labels, textStyle);
                        if (newIndex >= 0) {
                            Value = labels[newIndex];
                            UnityEditor.EditorUtility.SetDirty(node);
                        }
                        break;
                }
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Then Exit ", textStyle);
                var indices = new string[node.OutPoints.Count];
                for (int i = 0; i < indices.Length; i++) {
                    indices[i] = i.ToString();
                }
                Output = UnityEditor.EditorGUILayout.Popup(Output, indices, textStyle);
                if (GUILayout.Button("X", buttonStyle)) {
                    node.Remove(this);
                }
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
#endif
            }

            public bool IsTrue(RuntimeConditionNode node) {
                switch (Type) {
                    case ConditionType.Trigger:
                        switch (Comparison) {
                            case ComparisonType.Equals:
                            case ComparisonType.GreaterThan:
                            case ComparisonType.EqualsOrGreaterThan:
                            case ComparisonType.EqualsOrLessThan:
                                if (node.Graph.IsTriggerActive(Value)) {
                                    return true;
                                }
                                break;
                            case ComparisonType.LessThan:
                            case ComparisonType.NotEqualTo:
                                if (!node.Graph.IsTriggerActive(Value)) {
                                    return true;
                                }
                                break;
                        }
                        break;
                    
                }
                return false;
            }

            public void UseCondition(RuntimeConditionNode node) {
                switch (Type) {
                    case ConditionType.Trigger:
                        node.Graph.ResetTrigger(Value);
                        break;
                }
            }
        }

        public class RuntimeConditionNode : RuntimeStateNode {

            private ConditionNode _originalNode;

            public override RuntimeStateNode GetExitNode() {
                return GetConditionExitNode();
            }

            public RuntimeStateNode GetConditionExitNode() {
                for (int i = 0; i < _originalNode.Conditions.Count; i++) {
                    var condition = _originalNode.Conditions[i];
                    if (condition.IsTrue(this)) {
                        condition.UseCondition(this);
                        var endPoint = _originalNode.OutPoints[condition.Output];
                        var exitNode = Graph.OriginalGraph.GetConnectionEndpoint(endPoint);
                        if (exitNode != null) {
                            return Graph.GetRuntimeNode(exitNode.Id);
                        }
                    }
                }
                return null;
            }

            public RuntimeConditionNode(ConditionNode node, RuntimeStateGraph graph) : base(graph) {
                _originalNode = node;
            }

            public bool HasTrueCondition() {
                for (int i = 0; i < _originalNode.Conditions.Count; i++) {
                    if (_originalNode.Conditions[i].IsTrue(this)) {
                        return true;
                    }
                }
                return false;
            }

            public override bool TryComplete(float dt) {
                return HasTrueCondition();
            }
        }

        public enum ConditionType {
            Trigger,
        }

        public enum ComparisonType {
            Equals,
            NotEqualTo,
            GreaterThan,
            LessThan,
            EqualsOrGreaterThan,
            EqualsOrLessThan
        }
    }
}
