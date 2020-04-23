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

        private static List<ScriptableDatabase> _dbList = new List<ScriptableDatabase>();

        [MenuItem("Tools/Databases Window")]
        public static void ShowWindow() {
            var window = GetWindow<ScriptableDatabasesWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1600, 1200);
            window.Show();
        }
        
        protected override OdinMenuTree BuildMenuTree() {
            _dbList.Clear();
            var tree = new OdinMenuTree(true);
            tree.DefaultMenuStyle.IconSize = 28.00f;
            tree.Config.DrawSearchToolbar = true;
            
            var ids = AssetDatabase.FindAssets("t:ScriptableDatabase");
            for (int i = 0; i < ids.Length; i++) {
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableDatabase>(AssetDatabase.GUIDToAssetPath(ids[i]));
                if (obj != null) {
                    _dbList.Add(obj);
                }
            }
            
            RefreshDbs();
            
            for (int i = 0; i < _dbList.Count; i++) {
                var db = _dbList[i];
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

            tree.EnumerateTree().AddIcons<ICustomPreview>(m=> m.GetPreviewTexture());
            return tree;
        }

        private void RefreshDbs() {
            for (int i = 0; i < _dbList.Count; i++) {
                ScriptableDatabaseEditorExtension.RefreshDbProjectAssets(_dbList[i]);
            }
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
                    if (selected.Value is ScriptableDatabase db) {
                        GUILayout.Label(db.DbType?.Name);
                    }
                    else {
                        if (SirenixEditorGUI.ToolbarButton(new GUIContent("Rename " + selected.Name))) {
                            RenameWindow.Open(
                                (m) => {
                                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(selected.Value as UnityEngine.Object), ((RenameWindow) m).Text);
                                    AssetDatabase.SaveAssets();
                                    AssetDatabase.Refresh();
                                    BuildMenuTree();
                                }, "Rename", selected.Name);
                        }
                        if (SirenixEditorGUI.ToolbarButton(new GUIContent("Duplicate " + selected.Name))) {
                            var dupePath = AssetDatabase.GetAssetPath(selected.Value as UnityEngine.Object);
                            var newAssetPath = AssetDatabase.GenerateUniqueAssetPath(dupePath);
                            AssetDatabase.CopyAsset(dupePath, newAssetPath);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                            var newObj = AssetDatabase.LoadMainAssetAtPath(newAssetPath);
                            if (newObj != null) {
                                BuildMenuTree();
                                base.TrySelectMenuItemWithObject(newObj);
                            }
                        } 
                    }
                    if (SirenixEditorGUI.ToolbarButton(new GUIContent("Select " + selected.Name))) {
                        Selection.activeObject = selected.Value as UnityEngine.Object;
                    }
                }
                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Close"))) {
                    Close();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }
}