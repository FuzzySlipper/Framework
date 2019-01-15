using System;

using System.Text;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace PixelComrades {
    [CustomEditor(typeof(WorldControlMonitor), true)] public class WorldControlMonitorEditor : OdinEditor {

        public override void OnInspectorGUI() {
            //var script = (WorldControlMonitor) target;
            if (Application.isPlaying) {
                GUILayout.Label(string.Format("Current Active: {0}", WorldControlMonitor.Current != null && WorldControlMonitor.Current.WorldControlActive));
                GUILayout.Label(string.Format("Current GameObject: {0}", WorldControlMonitor.CurrentGameObject != null ? WorldControlMonitor.CurrentGameObject.name : "None"));
            }
            base.OnInspectorGUI();
        }
    }
}