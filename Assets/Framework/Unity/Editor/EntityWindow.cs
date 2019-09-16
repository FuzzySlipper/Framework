#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Sirenix.Utilities.Editor;
using Sirenix.Serialization;
using UnityEditor;
using Sirenix.Utilities;
using PixelComrades;
using Sirenix.OdinInspector;

public class EntityWindow : OdinEditorWindow {
    [MenuItem("Tools/Entity Window")]
    private static void OpenWindow() {
        var window = GetWindow<EntityWindow>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1200, 1000);
        window.Show();
    }

    private GUITable _entityTable;
    private string[] _entityNames = new string[0];
    private List<Entity> _activeEntities = new List<Entity>();
    private int _entityIndex = 0;
    private Vector2 _scrollPositionEntities;
    private Vector2 _scrollPositionComponents;

    //[HorizontalGroup]
    //[Button(ButtonSizes.Large)]
    private void ShowEntities() {
        var entities = EntityController.EntitiesArray;
        _activeEntities.Clear();
        foreach (Entity e in entities) {
            _activeEntities.Add(e);
        }
        //_entityTable = GUITable.Create<Entity>(_activeEntities, "Entities");
        _entityTable = GUITable.Create(_rows.Length, entities.Count, DrawElement, "Entity Fields", ColumnLabels, "Entities", RowLabels);
    }

    protected override void OnGUI() {
        base.OnGUI();
        var width = position.width /2;
        EditorGUILayout.BeginHorizontal();
        _scrollPositionEntities = EditorGUILayout.BeginScrollView(_scrollPositionEntities, GUILayout.Width(width));
        if (_entityTable == null) {
            ShowEntities();
        }
        _entityTable.DrawTable();
        EditorGUILayout.EndScrollView();
        _scrollPositionComponents = EditorGUILayout.BeginScrollView(_scrollPositionComponents, GUILayout.Width(width));
        ShowEntityComponents();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
    }

    private void ShowEntityComponents() {
        if (_entityNames.Length != _activeEntities.Count) {
            _entityNames = new string[_activeEntities.Count];
            for (int i = 0; i < _activeEntities.Count; i++) {
                _entityNames[i] = _activeEntities[i].Name;
            }
        }
        _entityIndex = EditorGUILayout.Popup(_entityIndex, _entityNames);
        if (!_activeEntities.HasIndex(_entityIndex)) {
            EditorGUILayout.LabelField("No Entity " + _entityIndex);
            return;
        }
        var entity = _activeEntities[_entityIndex];
        if (entity == null) {
            EditorGUILayout.LabelField("Null Entity " + _entityIndex);
            return;
        }
        EditorGUILayout.BeginVertical();
        for (int i = 0; i < entity.Tags.Tags.Length; i++) {
            if (entity.Tags.Tags[i] > 0) {
                EditorGUILayout.LabelField("Tag " + EntityTags.GetNameAt(i));
            }
        }
        foreach (var componentReference in entity.Components) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type: " + componentReference.Key.Name);
            System.Type type = componentReference.Key;
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var fields = type.GetFields(bindingFlags);
            System.Object instance = componentReference.Value.Get();
            List<object> fieldValues = null;
            if (instance != null) {
                fieldValues = fields.Select(field => field.GetValue(instance)).ToList();
            }
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < fields.Length; i++) {
                var field = fields[i].FieldType.Name;
                field = field.Replace("PixelComrades.", "");
                field = field.Replace("DungeonCrawler.", "");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(field);
                EditorGUILayout.LabelField(fields[i].Name);
                if (instance == null || !fieldValues.HasIndex(i) || fieldValues[i] == null) {
                    EditorGUILayout.EndHorizontal();
                    continue;
                }
                EditorGUILayout.LabelField(fieldValues[i].ToString());
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }

    private static string[] _rows = new[] {
        "ID", "Parent", "Tr", "Components", "Stats Count"
    };

    private void RowLabels(Rect arg1, int arg2) {
        if (GUI.Button(arg1, string.Format("{0} {1}:", arg2, _activeEntities.HasIndex(arg2) ? _activeEntities[arg2].Name : "Null"))) {
            if (_activeEntities.HasIndex(arg2)) {
                _entityIndex = arg2;
            }
        }
        //GUI.Label(arg1, string.Format("{0} {1}:", arg2, _activeEntities.HasIndex(arg2) ? _activeEntities[arg2].Name : "Null"));
    }

    private void ColumnLabels(Rect arg1, int arg2) {
        GUI.Label(arg1, _rows[arg2]);
    }

    private void DrawElement(Rect arg1, int arg2, int arg3) {
        var entity = _activeEntities.SafeAccess(arg3);
        if (entity == null) {
            GUI.Label(arg1, "Null");
            return;
        }
        var actor = entity.GetNode<CharacterNode>();
        switch (arg2) {
            case 0:
                GUI.Label(arg1, entity.Id.ToString());
                break;
            case 1:
                GUI.Label(arg1, entity.ParentId.ToString());
                break;
            case 2:
                GUI.Label(arg1, actor.Tr != null ? actor.Tr.name : "Null");
                break;
            case 3:
                GUI.Label(arg1, entity.Components.Count.ToString());
                break;
            case 4:
                GUI.Label(arg1, actor != null? actor.Stats.Count.ToString() : "Not Actor");
                break;
        }
    }

}
#endif
