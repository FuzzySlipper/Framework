using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Application = UnityEngine.Application;

namespace PixelComrades {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PlayerInput), true)]
    public class PlayerInputEditor : Editor {

        public override void OnInspectorGUI() {
            if (!Application.isPlaying) {
                return;
            }
            //var script = (PlayerInput) target;
            EditorGUILayout.LabelField(string.Format("Look {0}", PlayerInput.LookInput));
            EditorGUILayout.LabelField(string.Format("Move {0}", PlayerInput.MoveInput));
            EditorGUILayout.LabelField(string.Format("AllInputBlocked {0}", PlayerInput.AllInputBlocked));
            EditorGUILayout.LabelField(string.Format("MoveInputLocked {0}", PlayerInput.MoveInputLocked));
            EditorGUILayout.LabelField(string.Format("IsCursorOverUI {0}", PlayerInput.IsCursorOverUI));
            base.OnInspectorGUI();
        }
    }
}
