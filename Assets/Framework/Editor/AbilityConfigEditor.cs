using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace PixelComrades {
    // [CustomEditor(typeof(AbilityConfig), true)]
    // public class AbilityConfigEditor : OdinEditor {
    //
    //     private GenericMenu _addPhaseMenu;
    //     private GenericMenu _addHandlerMenu;
    //
    //     private AbilityConfig _script;
    //
    //     public override void OnInspectorGUI() {
    //         _script = (AbilityConfig) target;
    //         // for (int i = 0; i < _script.Phases.Count; i++) {
    //         //     var targetName = "Phase" + i;
    //         //     if (_script.Phases[i].name != targetName) {
    //         //         _script.Phases[i].name = targetName;
    //         //         EditorUtility.SetDirty(_script.Phases[i]);
    //         //     }
    //         // }
    //         // if (GUILayout.Button("Add Phase")) {
    //         //     if (_addPhaseMenu == null) {
    //         //         _addPhaseMenu = new GenericMenu();
    //         //         var types = ActionPhaseExtensions.PhaseTypes;
    //         //         for (var i = 0; i < types.Count; i++) {
    //         //             _addPhaseMenu.AddItem(new GUIContent(types[i].Name), false, CreatePhase, types[i]);
    //         //         }
    //         //     }
    //         //     _addPhaseMenu.ShowAsContext();
    //         // }
    //         // if (GUILayout.Button("Add Handler")) {
    //         //     if (_addHandlerMenu == null) {
    //         //         _addHandlerMenu = new GenericMenu();
    //         //         var types = ActionHandlerExtensions.HandlerTypes;
    //         //         for (var i = 0; i < types.Count; i++) {
    //         //             _addHandlerMenu.AddItem(new GUIContent(types[i].Name), false, CreateHandler, types[i]);
    //         //         }
    //         //     }
    //         //     _addHandlerMenu.ShowAsContext();
    //         // }
    //         base.OnInspectorGUI();
    //     }
    //
    //     // public void CreatePhase(object obj) {
    //     //     var targetType = obj as Type;
    //     //     if (targetType == null) {
    //     //         return;
    //     //     }
    //     //     var newObj = CreateInstance(targetType);
    //     //     newObj.name = targetType.Name;
    //     //     AssetDatabase.AddObjectToAsset(newObj, _script);
    //     //     AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newObj));
    //     //     _script.Phases.Add((ActionPhases)newObj);
    //     //     AssetDatabase.Refresh();
    //     // }
    //     //
    //     // public void CreateHandler(object obj) {
    //     //     var targetType = obj as Type;
    //     //     if (targetType == null) {
    //     //         return;
    //     //     }
    //     //     var newObj = CreateInstance(targetType);
    //     //     newObj.name = targetType.Name;
    //     //     AssetDatabase.AddObjectToAsset(newObj, _script);
    //     //     AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newObj));
    //     //     _script.Handlers.Add((ActionHandler) newObj);
    //     //     AssetDatabase.Refresh();
    //     // }
    // }

    // [CustomEditor(typeof(ActionPhases), true)]
    // public class ActionPhasesEditor : OdinEditor {
    //     public override void OnInspectorGUI() {
    //         var script = (ActionPhases) target;
    //         if (GUILayout.Button("Delete")) {
    //             var objects = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(target));
    //             for (int i = 0; i < objects.Length; i++) {
    //                 if (objects[i] is IActionConfig actionConfig) {
    //                     actionConfig.Phases.Remove(script);
    //                     DestroyImmediate(script, true);
    //                     EditorUtility.SetDirty(objects[i]);
    //                     AssetDatabase.Refresh();
    //                     return;
    //                 }
    //             }
    //         }
    //         base.OnInspectorGUI();
    //     }
    // }
    //
    // [CustomEditor(typeof(ActionHandler), true)]
    // public class ActionHandlerEditor : OdinEditor {
    //     public override void OnInspectorGUI() {
    //         var script = (ActionHandler) target;
    //         if (GUILayout.Button("Delete")) {
    //             var objects = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(target));
    //             for (int i = 0; i < objects.Length; i++) {
    //                 if (objects[i] is IActionConfig actionConfig) {
    //                     actionConfig.Handlers.Remove(script);
    //                     DestroyImmediate(script, true);
    //                     EditorUtility.SetDirty(objects[i]);
    //                     AssetDatabase.Refresh();
    //                     return;
    //                 }
    //             }
    //         }
    //         base.OnInspectorGUI();
    //     }
    // }


    public static class ActionPhaseExtensions {
        private static List<System.Type> _phaseTypes = new List<System.Type>();

        public static List<System.Type> PhaseTypes {
            get {
                if (_phaseTypes.Count == 0) {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    for (int a = 0; a < assemblies.Length; a++) {
                        var types = assemblies[a].GetTypes();
                        for (int t = 0; t < types.Length; t++) {
                            var type = types[t];
                            if (!type.IsAbstract && type.IsSubclassOf(typeof(ActionPhases))) {
                                _phaseTypes.Add(type);
                            }
                        }
                    }
                }
                return _phaseTypes;
            }
        }
    }

    public static class ActionHandlerExtensions {
        private static List<System.Type> _handlerTypes = new List<System.Type>();

        public static List<System.Type> HandlerTypes {
            get {
                if (_handlerTypes.Count == 0) {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    for (int a = 0; a < assemblies.Length; a++) {
                        var types = assemblies[a].GetTypes();
                        for (int t = 0; t < types.Length; t++) {
                            var type = types[t];
                            if (!type.IsAbstract && type.IsSubclassOf(typeof(ActionHandler))) {
                                _handlerTypes.Add(type);
                            }
                        }
                    }
                }
                return _handlerTypes;
            }
        }
    }
}