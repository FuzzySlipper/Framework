// Drop this in Assets/Editor, and you will get a new menu item.
// Change the prefix to what you want.
// Backup your project.
// Use menu item to apply changes.

using UnityEngine;
using UnityEditor;

static class PrefixSelectionChildren {
    const string prefix = "PREFIX_"; // Change to whatever you want to prefix with
    [MenuItem("Tools/Apply Prefix to Children")]
    public static void ApplyPrefix() {
        if (!EditorUtility.DisplayDialog("Child Prefix Warning",
            "You are about to rename children of your current selection. Undo is not possible.\n\nAre you sure you want to continue?",
            "Yes, I understand the risk",
            "No, I changed my mind"))
            return;

        GameObject[] gos = Selection.gameObjects;
        foreach (GameObject go in gos) {
            var children = go.GetComponentsInChildren(typeof(Transform));
            foreach (Transform child in children) {
                // Don't apply to root object.
                if (child == go.transform)
                    continue;

                child.name = prefix + child.name;
            }
        }
    }
}