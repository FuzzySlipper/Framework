using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class SpawnOnEvent : IComponent {

        public ActionState Start { get; }
        public ActionState End { get; }
        public PrefabEntity Prefab { get; }
        public bool Parent { get; }

        public PrefabEntity ActiveGameObject;

        public SpawnOnEvent(ActionState start, ActionState end, bool parent, PrefabEntity prefab) {
            Start = start;
            End = end;
            Prefab = prefab;
            Parent = parent;
        }

        public SpawnOnEvent(SerializationInfo info, StreamingContext context) {
            Start = info.GetValue(nameof(Start), Start);
            End = info.GetValue(nameof(End), End);
            Parent = info.GetValue(nameof(Parent), Parent);
            ActiveGameObject = Serializer.GetPrefabEntity(info.GetValue(nameof(ActiveGameObject), -1));
            Prefab = ItemPool.GetReferencePrefab(info.GetValue(nameof(Prefab), -1));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Start), Start);
            info.AddValue(nameof(End), End);
            info.AddValue(nameof(Parent), Parent);
            info.AddValue(nameof(ActiveGameObject), ActiveGameObject != null ? ActiveGameObject.Metadata.SerializationId : -1);
            info.AddValue(nameof(Prefab), Prefab.PrefabId);
        }
    }
}
