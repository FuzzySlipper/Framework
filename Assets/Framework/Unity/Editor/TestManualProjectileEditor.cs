using PixelComrades;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TestManualProjectile))]
public class TestManualProjectileEditor : Editor {

    public override void OnInspectorGUI() {
        var script = (TestManualProjectile)target;
        if (GUILayout.Button("Test")) {
            script.Test(false);
        }
        if (GUILayout.Button("Test With Collision")) {
            script.Test(true);
        }
        if (GUILayout.Button("Test With Reset")) {
            script.transform.position = script.LastTestStart;
            script.transform.rotation = Quaternion.identity;
            script.Test(false);
        }
        if (GUILayout.Button("Reset Position")) {
            script.transform.position = script.LastTestStart;
            script.transform.rotation = Quaternion.identity;
        }
        base.OnInspectorGUI();
    }
}