using PixelComrades;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TweenAnimator), true), CanEditMultipleObjects]
public class TweenAnimatorEditor : Editor {

    public override void OnInspectorGUI() {
        var script = (TweenAnimator)target;
        if (GUILayout.Button("Test")) {
            TimeManager.StartUnscaled(script.PlayAnimation()); 
        }
        if (script.Chain != null) {
            GUILayout.Label(string.Format("Chain: {0}", script.Chain.Description));
        }
        base.OnInspectorGUI();
    }
}