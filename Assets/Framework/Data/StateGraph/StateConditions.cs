using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PixelComrades {

    [System.Serializable]
    public class ConditionExit : ConditionChecker {

        public void DrawGui(StateGraphNode node, GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("If", textStyle);
            DrawComparison(textStyle, buttonStyle);
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (DrawType(node.Graph, textStyle, buttonStyle)) {
                UnityEditor.EditorUtility.SetDirty(node);
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
    }
    
    [System.Serializable]
    public abstract class ConditionChecker {
        public ConditionType Type = ConditionType.StringVariable;
        public ComparisonType Comparison;
        public string Value = "Value";
        public string VariableName ="Variable";
        public int Output = 0;

        public RuntimeConditionChecker GetRuntime() {
            switch (Type) {
                case ConditionType.Trigger:
                    return new TriggerChecker(this);
                case ConditionType.StringVariable:
                    return new StringVariableChecker(this);
                case ConditionType.BoolVariable:
                    return new BoolVariableChecker(this);
                case ConditionType.IntVariable:
                    return new IntVariableChecker(this);
                case ConditionType.FloatVariable:
                    return new FloatVariableChecker(this);
                case ConditionType.EntityTag:
                    return new TagVariableChecker(this);
                case ConditionType.IsAttacking:
                    return new IsAttackingChecker(this);
            }
            return null;
        }

        public void DrawComparison(GUIStyle textStyle, GUIStyle buttonStyle) {
#if UNITY_EDITOR
            Type = (ConditionType) UnityEditor.EditorGUILayout.EnumPopup(Type, buttonStyle);
            Comparison = (ComparisonType) UnityEditor.EditorGUILayout.EnumPopup(Comparison, buttonStyle);
#endif
        }

        public bool DrawType(StateGraph graph, GUIStyle textStyle, GUIStyle buttonStyle) {
            bool changed = false;
#if UNITY_EDITOR
            switch (Type) {
                case ConditionType.Trigger:
                case ConditionType.EntityTag:
                    break;
                case ConditionType.IsAttacking:
                    GUILayout.Label("IsAttacking", textStyle);
                    break;
                default:
                    var graphLabels = GraphVariables.GetValues();
                    var index = System.Array.IndexOf(graphLabels, VariableName);
                    var newVar = UnityEditor.EditorGUILayout.Popup(index, graphLabels, buttonStyle, new []{GUILayout.MaxWidth
                    (StateGraphNode.DefaultNodeSize.x * 0.5f)});
                    if (newVar != index) {
                        VariableName = graphLabels[newVar];
                    }
                    break;
            }
            switch (Type) {
                case ConditionType.IsAttacking:
                    bool.TryParse(Value, out bool oldBool);
                    if (GUILayout.Button(oldBool.ToString(), buttonStyle)) {
                        Value = (!oldBool).ToString();
                    }
                    break;
                case ConditionType.Trigger:
                    var labels = graph.GlobalTriggers.Select(t => t.Key).ToArray();
                    var index = System.Array.IndexOf(labels, Value);
                    var newIndex = UnityEditor.EditorGUILayout.Popup(index, labels, buttonStyle);
                    if (newIndex >= 0) {
                        Value = labels[newIndex];
                        changed = true;
                    }
                    break;
                case ConditionType.StringVariable:
                    Value = GUILayout.TextField(Value, buttonStyle);
                    break;
                case ConditionType.BoolVariable:
                    bool.TryParse(Value, out bool oldValue);
                    if (GUILayout.Button(oldValue.ToString())) {
                        Value = (!oldValue).ToString();
                    }
                    break;
                case ConditionType.EntityTag:
                    int.TryParse(Value, out int oldTagInt);
                    var animationLabels = AnimationEvents.GetValues();
                    var newTag = UnityEditor.EditorGUILayout.Popup(oldTagInt, animationLabels);
                    if (newTag != oldTagInt) {
                        Value = newTag.ToString();
                    }
                    break;
                case ConditionType.FloatVariable:
                    float.TryParse(Value, out float oldFloat);
                    var newFloatStr = GUILayout.TextField(Value, buttonStyle);
                    if (float.TryParse(newFloatStr, out var newFloat) && Math.Abs(newFloat - oldFloat) > 0.001f) {
                        Value = newFloatStr;
                    }
                    break;
                case ConditionType.IntVariable:
                    int.TryParse(Value, out int oldInt);
                    var newIntStr = GUILayout.TextField(Value, buttonStyle);
                    if (int.TryParse(newIntStr, out var newInt) && newInt != oldInt) {
                        Value = newIntStr;
                    }
                    break;
            }
#endif
            return changed;
        }
    }

    public abstract class RuntimeConditionChecker {
        
        public ConditionChecker Original { get; }
        public abstract bool IsTrue(RuntimeStateNode node);

        protected RuntimeConditionChecker(ConditionChecker condition) {
            Original = condition;
        }
    }

    public class TriggerChecker : RuntimeConditionChecker {
        public TriggerChecker(ConditionChecker condition) : base(condition) {}

        public override bool IsTrue(RuntimeStateNode node) {
            switch (Original.Comparison) {
                case ComparisonType.Equals:
                case ComparisonType.GreaterThan:
                case ComparisonType.EqualsOrGreaterThan:
                case ComparisonType.EqualsOrLessThan:
                    if (node.Graph.IsGlobalTriggerActive(Original.Value)) {
                        return true;
                    }
                    break;
                case ComparisonType.LessThan:
                case ComparisonType.NotEqualTo:
                    if (!node.Graph.IsGlobalTriggerActive(Original.Value)) {
                        return true;
                    }
                    break;
            }
            return false;
        }
    }

    public class StringVariableChecker : RuntimeConditionChecker {
        public StringVariableChecker(ConditionChecker condition) : base(condition) {
        }

        public override bool IsTrue(RuntimeStateNode node) {
            switch (Original.Comparison) {
                case ComparisonType.Equals:
                case ComparisonType.GreaterThan:
                case ComparisonType.EqualsOrGreaterThan:
                case ComparisonType.EqualsOrLessThan:
                    return node.Graph.GetVariable<string>(Original.VariableName) == Original.Value;
                case ComparisonType.LessThan:
                case ComparisonType.NotEqualTo:
                    return node.Graph.GetVariable<string>(Original.VariableName) != Original.Value;
            }
            return false;
        }
    }

    public class BoolVariableChecker : RuntimeConditionChecker {
        public bool Value;
        public BoolVariableChecker(ConditionChecker condition) : base(condition) {
            bool.TryParse(condition.Value, out Value);
        }

        public override bool IsTrue(RuntimeStateNode node) {
            switch (Original.Comparison) {
                case ComparisonType.Equals:
                case ComparisonType.GreaterThan:
                case ComparisonType.EqualsOrGreaterThan:
                case ComparisonType.EqualsOrLessThan:
                    return node.Graph.GetVariable<bool>(Original.VariableName) == Value;
                case ComparisonType.LessThan:
                case ComparisonType.NotEqualTo:
                    return node.Graph.GetVariable<bool>(Original.VariableName) != Value;
            }
            return false;
        }
    }

    public class IntVariableChecker : RuntimeConditionChecker {
        public int Value;

        public IntVariableChecker(ConditionChecker condition) : base(condition) {
            int.TryParse(condition.Value, out Value);
        }

        public override bool IsTrue(RuntimeStateNode node) {
            switch (Original.Comparison) {
                case ComparisonType.Equals:
                case ComparisonType.GreaterThan:
                case ComparisonType.EqualsOrGreaterThan:
                case ComparisonType.EqualsOrLessThan:
                    return node.Graph.GetVariable<int>(Original.VariableName) == Value;
                case ComparisonType.LessThan:
                case ComparisonType.NotEqualTo:
                    return node.Graph.GetVariable<int>(Original.VariableName) != Value;
            }
            return false;
        }
    }

    public class TagVariableChecker : RuntimeConditionChecker {
        public int Value;

        public TagVariableChecker(ConditionChecker condition) : base(condition) {
            int.TryParse(condition.Value, out Value);
        }

        public override bool IsTrue(RuntimeStateNode node) {
            switch (Original.Comparison) {
                case ComparisonType.Equals:
                case ComparisonType.GreaterThan:
                case ComparisonType.EqualsOrGreaterThan:
                case ComparisonType.EqualsOrLessThan:
                    return node.Graph.Entity.Tags.Contain(Value);
                case ComparisonType.LessThan:
                case ComparisonType.NotEqualTo:
                    return !node.Graph.Entity.Tags.Contain(Value);
            }
            return false;
        }
    }

    public class FloatVariableChecker : RuntimeConditionChecker {
        public float Value;

        public FloatVariableChecker(ConditionChecker condition) : base(condition) {
            float.TryParse(condition.Value, out Value);
        }

        public override bool IsTrue(RuntimeStateNode node) {
            switch (Original.Comparison) {
                case ComparisonType.Equals:
                case ComparisonType.GreaterThan:
                case ComparisonType.EqualsOrGreaterThan:
                case ComparisonType.EqualsOrLessThan:
                    return Math.Abs(node.Graph.GetVariable<float>(Original.VariableName) - Value) < 0.001f;
                case ComparisonType.LessThan:
                case ComparisonType.NotEqualTo:
                    return Math.Abs(node.Graph.GetVariable<float>(Original.VariableName) - Value) > 0.001f;
            }
            return false;
        }
    }

    public class IsAttackingChecker : RuntimeConditionChecker {
        public bool Value;

        public IsAttackingChecker(ConditionChecker condition) : base(condition) {
            bool.TryParse(condition.Value, out Value);
        }

        public override bool IsTrue(RuntimeStateNode node) {
            bool isAttacking = false;
            if (node.Graph.GetVariable<bool>(GraphVariables.Attacking)) {
                isAttacking = true;
                var action = node.Graph.Entity.Get<CurrentAction>().Value;
                if (action == null) {
                    isAttacking = false;
                }
                else {
                    var actionEntity = action.Entity;
                    for (int i = 0; i < action.Config.Costs.Count; i++) {
                        if (!action.Config.Costs[i].CanAct(node.Graph.Entity, actionEntity)) {
                            isAttacking = false;
                            break;
                        }
                    }
                }
            }
            switch (Original.Comparison) {
                case ComparisonType.Equals:
                case ComparisonType.GreaterThan:
                case ComparisonType.EqualsOrGreaterThan:
                case ComparisonType.EqualsOrLessThan:
                    return isAttacking == Value;
                case ComparisonType.LessThan:
                case ComparisonType.NotEqualTo:
                    return isAttacking != Value;
            }
            return isAttacking;
        }
    }
    

    public enum ConditionType {
        Trigger,
        StringVariable,
        BoolVariable,
        IntVariable,
        FloatVariable,
        EntityTag,
        IsAttacking
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
