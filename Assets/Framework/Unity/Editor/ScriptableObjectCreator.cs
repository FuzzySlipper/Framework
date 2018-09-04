using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class ScriptableObjectCreator : OdinMenuEditorWindow {
    private static HashSet<Type> scriptableObjectTypes = AssemblyUtilities.GetTypes(AssemblyTypeFlags.UserEditorTypes | AssemblyTypeFlags.UserTypes)
        .Where(
            t =>
                t.IsClass &&
                typeof(ScriptableObject).IsAssignableFrom(t) &&
                !typeof(EditorWindow).IsAssignableFrom(t) &&
                !typeof(Editor).IsAssignableFrom(t))
        .ToHashSet();
    private ScriptableObject previewObject;
    private Vector2 scroll;
    private string targetFolder;
    private Type SelectedType { 
        get {
            var m = MenuTree.Selection.LastOrDefault();
            return m == null ? null : m.Value as Type;
        }
    }

    private void CreateAsset() {
        if (previewObject) {
            var dest = targetFolder + "/" + MenuTree.Selection.First().Name + ".asset";
            dest = AssetDatabase.GenerateUniqueAssetPath(dest);
            AssetDatabase.CreateAsset(previewObject, dest);
            AssetDatabase.Refresh();
            Selection.activeObject = previewObject;
            EditorApplication.delayCall += Close;
        }
    }

    private string GetMenuPathForType(Type t) {
        if (t != null && scriptableObjectTypes.Contains(t)) {
            var name = t.Name.Split('`').First().SplitPascalCase();
            return GetMenuPathForType(t.BaseType) + "/" + name;
        }
        return "";
    }

    [MenuItem("Assets/Create Scriptable Object", priority = -1000)]
    private static void ShowDialog() {
        var path = "Assets";
        var obj = Selection.activeObject;
        if (obj && AssetDatabase.Contains(obj)) {
            path = AssetDatabase.GetAssetPath(obj);
            if (!Directory.Exists(path)) {
                path = Path.GetDirectoryName(path);
            }
        }
        var window = CreateInstance<ScriptableObjectCreator>();
        window.ShowUtility();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
        window.titleContent = new GUIContent(path);
        window.targetFolder = path.Trim('/');
    }

    protected override OdinMenuTree BuildMenuTree() {
        MenuWidth = 270;
        WindowPadding = Vector4.zero;
        OdinMenuTree tree = new OdinMenuTree(false);
        var search = tree.Config.SearchFunction;
        tree.Config.SearchFunction = t => t.Value != null && !(t.Value as Type).IsAbstract && search(t);
        tree.Config.DrawSearchToolbar = true;
        tree.DefaultMenuStyle = OdinMenuStyle.TreeViewStyle;
        tree.AddRange(scriptableObjectTypes.Where(x => !x.IsAbstract), GetMenuPathForType).AddThumbnailIcons();
        tree.SortMenuItemsByName();
        tree.Selection.SelectionConfirmed += x => CreateAsset();
        tree.Selection.SelectionChanged += e => {
            if (previewObject && !AssetDatabase.Contains(previewObject)) {
                DestroyImmediate(previewObject);
            }
            if (e != SelectionChangedType.ItemAdded) {
                return;
            }
            var t = SelectedType;
            if (t != null && !t.IsAbstract) {
                previewObject = CreateInstance(t);
            }
        };
        return tree;
    }

    protected override void DrawEditor(int index) {
        scroll = GUILayout.BeginScrollView(scroll);
        {
            base.DrawEditor(index);
        }
        GUILayout.EndScrollView();
        if (previewObject) {
            GUILayout.FlexibleSpace();
            SirenixEditorGUI.HorizontalLineSeparator(1);
            if (GUILayout.Button("Create Asset", GUILayoutOptions.Height(30))) {
                CreateAsset();
            }
        }
    }

    protected override IEnumerable<object> GetTargets() {
        yield return previewObject;
    }
}