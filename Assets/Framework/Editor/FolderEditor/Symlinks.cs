using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Parabox {
    /**
     *	An editor utility for easily creating symlinks in your project.
     *
     *	Adds a Menu item under `Assets/Create/Folder (Symlink)`, and 
     *	draws a small indicator in the Project view for folders that are
     *	symlinks.
     */
    [InitializeOnLoad]
    public static class SymlinkUtility {
        // FileAttributes that match a junction folder.
        private const FileAttributes FolderSymlinkAttribs = FileAttributes.Directory | FileAttributes.ReparsePoint;

        // Style used to draw the symlink indicator in the project view.
        private static GUIStyle _symlinkMarkerStyle;

        /**
         *	Static constructor subscribes to projectWindowItemOnGUI delegate.
         */
        static SymlinkUtility() {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGui;
        }

        private static GUIStyle SymlinkMarkerStyle {
            get {
                if (_symlinkMarkerStyle == null) {
                    _symlinkMarkerStyle = new GUIStyle(EditorStyles.label);
                    _symlinkMarkerStyle.normal.textColor = new Color(.2f, .8f, .2f, .8f);
                    _symlinkMarkerStyle.alignment = TextAnchor.MiddleRight;
                }
                return _symlinkMarkerStyle;
            }
        }

        /**
         *	Add a menu item in the Assets/Create category to add symlinks to directories.
         */
        [MenuItem("Assets/Create/Folder (Symlink)", false, 20)]
        private static void DoTheSymlink() {
            string sourceFolderPath = EditorUtility.OpenFolderPanel("Select Folder Source", "", "");
            if (sourceFolderPath.Contains(Application.dataPath)) {
                Debug.LogWarning("Cannot create a symlink to folder in your project!");
                return;
            }

            // Cancelled dialog
            if (string.IsNullOrEmpty(sourceFolderPath)) {
                return;
            }
            string sourceFolderName = sourceFolderPath.Split('/', '\\').LastOrDefault();
            if (string.IsNullOrEmpty(sourceFolderName)) {
                Debug.LogWarning("Couldn't deduce the folder name?");
                return;
            }
            Object uobject = Selection.activeObject;
            string targetPath = uobject != null ? AssetDatabase.GetAssetPath(uobject) : "";
            if (string.IsNullOrEmpty(targetPath)) {
                targetPath = "Assets";
            }
            FileAttributes attribs = File.GetAttributes(targetPath);
            if ((attribs & FileAttributes.Directory) != FileAttributes.Directory) {
                targetPath = Path.GetDirectoryName(targetPath);
            }
            targetPath = string.Format("{0}/{1}", targetPath, sourceFolderName);
            if (Directory.Exists(targetPath)) {
                Debug.LogWarning(string.Format("A folder already exists at this location, aborting link.\n{0} -> {1}", sourceFolderPath, targetPath));
                return;
            }
#if UNITY_EDITOR_WIN
            Process cmd = Process.Start("CMD.exe", string.Format("/C mklink /J \"{0}\" \"{1}\"", targetPath, sourceFolderPath));
            cmd.WaitForExit();
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
// @todo
#endif

            // UnityEngine.Debug.Log(string.Format("Created symlink: {0} <=> {1}", targetPath, sourceFolderPath));
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        /**
         *	Draw a little indicator if folder is a symlink
         */
        private static void OnProjectWindowItemGui(string guid, Rect r) {
            try {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path)) {
                    FileAttributes attribs = File.GetAttributes(path);
                    if ((attribs & FolderSymlinkAttribs) == FolderSymlinkAttribs) {
                        GUI.Label(r, "<=>", SymlinkMarkerStyle);
                    }
                }
            }
            catch {
            }
        }
    }
}