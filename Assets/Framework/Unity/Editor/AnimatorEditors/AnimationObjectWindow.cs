using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace PixelComrades {
    public class AnimationObjectWindow : EditorWindow {
        private AnimationObject _currentAction;

        public static void ShowWindow() {
            GetWindow(typeof(AnimationObjectWindow));
        }

        public void Set(AnimationObject action) {
            _currentAction = action;
        }

        private void OnGUI() {
            if (_currentAction != null) {
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return) {
                    Close();
                }

                _currentAction.DrawEditorGui();
            }
            else {
                GUILayout.Label("Select action");
            }
        }
    }
}
