using System.Collections.Generic;
using PixelComrades;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (UIRadialMenu))]
public class UIRadialMenuEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        var rm = (UIRadialMenu) target;
        if (Application.isPlaying) {
            return;
        }
        if (GUILayout.Button("Visualize Arrangement")) {
            rm.TestItemsEditor();
        }
        //if (GUILayout.Button("Test Animate In")) {
        //    rm.FindElementsEditor();
        //    TimeManager.StartUnscaled(rm.AnimateOpen());
        //}
        //if (GUILayout.Button("Test Animate Out")) {
        //    rm.FindElementsEditor();
        //    TimeManager.StartUnscaled(rm.CloseRadial());
        //}
    }
}