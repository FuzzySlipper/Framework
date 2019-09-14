using System.IO;
using UnityEngine;
using UnityEditor;

//using SyntaxTree.VisualStudio.Unity.Bridge;
//
//[InitializeOnLoad]
//public static class ReferenceRemovalProjectHook {
//    static ReferenceRemovalProjectHook() {
//        const string references = "\r\n    <Reference Include=\"Boo.Lang\" />\r\n    <Reference Include=\"UnityScript.Lang\" />";
//
//        ProjectFilesGenerator.ProjectFileGeneration += (string name, string content) =>
//            content.Replace(references, "");
//    }
//}

    public static class CustomTextEditMenuItem {

        private const string TextEditor = "C:\\Program Files\\Microsoft VS Code\\Code.exe";

        [MenuItem("Assets/Open in Text Editor...")] static void OpenInTextEditor() {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), AssetDatabase.GetAssetPath(Selection.activeObject));
            System.Diagnostics.Process.Start(TextEditor, filePath);
        }

        [MenuItem("Assets/Open in Text Editor...", true)] static bool ValidateOpenInTextEditor() {
            return AssetDatabase.GetAssetPath(Selection.activeObject).EndsWith(".txt", System.StringComparison.OrdinalIgnoreCase);
        }
    }