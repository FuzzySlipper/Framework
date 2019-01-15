using UnityEditor;
using UnityEngine;

public class FindMissingScriptsRecursively : EditorWindow {
    private static int _goCount, _componentsCount, _missingCount;

    private static void FindInGo(GameObject g) {
        _goCount++;
        Component[] components = g.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++) {
            _componentsCount++;
            if (components[i] == null) {
                _missingCount++;
                string s = g.name;
                Transform t = g.transform;
                while (t.parent != null) {
                    s = t.parent.name + "/" + s;
                    t = t.parent;
                }
                Debug.Log(s + " has an empty script attached in position: " + i, g);
            }
        }
        // Now recurse through each child GO (if there are any):
        foreach (Transform childT in g.transform) {
            //Debug.Log("Searching " + childT.name  + " " );
            FindInGo(childT.gameObject);
        }
    }

    private static void FindInSelected() {
        GameObject[] go = Selection.gameObjects;
        _goCount = 0;
        _componentsCount = 0;
        _missingCount = 0;
        foreach (GameObject g in go) {
            FindInGo(g);
        }
        Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", _goCount, _componentsCount, _missingCount));
    }

    public void OnGUI() {
        if (GUILayout.Button("Find Missing Scripts in selected GameObjects")) {
            FindInSelected();
        }
    }

    [MenuItem("Window/FindMissingScriptsRecursively")]
    public static void ShowWindow() {
        GetWindow(typeof(FindMissingScriptsRecursively));
    }
}