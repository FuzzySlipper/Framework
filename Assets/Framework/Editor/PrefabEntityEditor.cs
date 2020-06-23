using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
<<<<<<< HEAD:Assets/Framework/Editor/WorldEntityEditor.cs
=======
using Sirenix.OdinInspector.Editor;
>>>>>>> FirstPersonAction:Assets/Framework/Editor/PrefabEntityEditor.cs
using UnityEditor;

namespace PixelComrades {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PrefabEntity), true)]
<<<<<<< HEAD:Assets/Framework/Editor/WorldEntityEditor.cs
    public class WorldEntityEditor : Editor {
=======
    public class PrefabEntityEditor : OdinEditor {
>>>>>>> FirstPersonAction:Assets/Framework/Editor/PrefabEntityEditor.cs
        
        public override void OnInspectorGUI() {
            var script = (PrefabEntity)target;
            EditorGUILayout.LabelField(string.Format("Active {0}", script.SceneActive));
            EditorGUILayout.LabelField(string.Format("Sector {0}", script.SectorPosition));
            EditorGUILayout.LabelField(string.Format("Entity {0}: {1}", script.EntityID, script.Entity?.DebugId));
            base.OnInspectorGUI();
        }
    }
}
