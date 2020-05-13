using UnityEngine;
using UnityEditor;

public class SetTransformToLocalZero : Editor {

    [MenuItem("Window/Snap to Local Zero #%z")]
    static void DropToYMinus() {
        if (Selection.activeTransform != null) {
            Selection.activeTransform.localPosition = Vector3.zero;
            Selection.activeTransform.localRotation = Quaternion.identity;
        }
    }

    [MenuItem("Window/Snap to Y Zero &%z")]
    static void DropToYZero() {
        if (Selection.activeTransform != null) {
            Selection.activeTransform.position = new Vector3(
                Selection.activeTransform.position.x,0,
                Selection.activeTransform.position.z );
            
        }
    }
}
