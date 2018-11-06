using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace PixelComrades {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PrefabEntity), true)]
    public class WorldEntityEditor : Editor {
        
        public override void OnInspectorGUI() {
            var script = (PrefabEntity)target;
            EditorGUILayout.LabelField(string.Format("Active {0}", script.SceneActive));
            EditorGUILayout.LabelField(string.Format("Sector {0}", script.SectorPosition));
            EditorGUILayout.LabelField(string.Format("Entity {0}", script.EntityID));
            base.OnInspectorGUI();
        }
    }
}
