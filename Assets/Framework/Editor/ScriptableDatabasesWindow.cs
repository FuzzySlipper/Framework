using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;

namespace PixelComrades {
    public class ScriptableDatabasesWindow : OdinMenuEditorWindow {
        [MenuItem("Tools/Databases Window")]
        public static void ShowWindow() {
            var window = GetWindow<ScriptableDatabasesWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1600, 1200);
            window.Show();
        }
        
        protected override OdinMenuTree BuildMenuTree() {
            var tree = new OdinMenuTree(true);
            tree.DefaultMenuStyle.IconSize = 28.00f;
            tree.Config.DrawSearchToolbar = true;
            
            var ids = AssetDatabase.FindAssets("t:ScriptableDatabase");
            var dbList = new List<ScriptableDatabase>();
            for (int i = 0; i < ids.Length; i++) {
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableDatabase>(AssetDatabase.GUIDToAssetPath(ids[i]));
                if (obj != null) {
                    dbList.Add(obj);
                }
            }
            for (int i = 0; i < dbList.Count; i++) {
                var db = dbList[i];
                var dbTable = db.GetEditorWindow();
                if (dbTable != null) {
                    tree.Add(db.name, dbTable);
                }
                else {
                    tree.AddObjectAtPath(db.name, db, false);
                }
                var dbObjs = db.AllObjects;
                if (dbObjs == null) {
                    continue;
                }
                foreach (var dbObj in dbObjs) {
                    if (dbObj == null) {
                        continue;
                    }
                    tree.AddAssetAtPath(db.name + "/" + dbObj.name, AssetDatabase.GetAssetPath(dbObj));
                }
            }
            // tree.AddAllAssetsAtPath("", "Assets/Plugins/Sirenix/Demos/SAMPLE - RPG Editor/Items", typeof(Item), true)
            //     .ForEach(this.AddDragHandles);
            // Add drag handles to items, so they can be easily dragged into the inventory if characters etc...
            //tree.EnumerateTree().Where(x => x.Value as Item).ForEach(AddDragHandles);

            tree.EnumerateTree().AddIcons<ICustomPreview>(x => GUIHelper.GetAssetThumbnail(x.Preview, x.Preview.GetType(), true));
            return tree;
        }

        private void AddDragHandles(OdinMenuItem menuItem) {
            menuItem.OnDrawItem += x => DragAndDropUtilities.DragZone(menuItem.Rect, menuItem.Value, false, false);
        }

        protected override void OnBeginDrawEditors() {
            var selected = this.MenuTree.Selection.FirstOrDefault();
            var toolbarHeight = this.MenuTree.Config.SearchToolbarHeight;

            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight); {
                if (selected != null) {
                    GUILayout.Label(selected.Name);
                }
                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Close"))) {
                    Close();
                }
                // if ()
                // {
                //     Sirenix.OdinInspector.Demos.RPGEditor.ScriptableObjectCreator.ShowDialog<Item>("Assets/Plugins/Sirenix/Demos/Sample - RPG Editor/Items", obj =>
                //     {
                //         obj.Name = obj.name;
                //         base.TrySelectMenuItemWithObject(obj); // Selects the newly created item in the editor
                //     });
                // }
                //
                // if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Character")))
                // {
                //     Sirenix.OdinInspector.Demos.RPGEditor.ScriptableObjectCreator.ShowDialog<Character>("Assets/Plugins/Sirenix/Demos/Sample - RPG Editor/Character", obj =>
                //     {
                //         obj.Name = obj.name;
                //         base.TrySelectMenuItemWithObject(obj); // Selects the newly created item in the editor
                //     });
                // }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }
}