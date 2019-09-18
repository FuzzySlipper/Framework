using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class SpawnOnEvent : IComponent {

        public ActionStateEvents StartEvent { get; }
        public ActionStateEvents EndEvent { get; }
        public PrefabEntity Prefab { get; }
        public bool Parent { get; }

        public PrefabEntity ActiveGameObject;

        public SpawnOnEvent(ActionStateEvents startEvent, ActionStateEvents endEvent, bool parent, PrefabEntity prefab) {
            StartEvent = startEvent;
            EndEvent = endEvent;
            Prefab = prefab;
            Parent = parent;
        }

        public SpawnOnEvent(SerializationInfo info, StreamingContext context) {
            StartEvent = info.GetValue(nameof(StartEvent), StartEvent);
            EndEvent = info.GetValue(nameof(EndEvent), EndEvent);
            Parent = info.GetValue(nameof(Parent), Parent);
            ActiveGameObject = Serializer.GetPrefabEntity(info.GetValue(nameof(ActiveGameObject), -1));
            Prefab = ItemPool.GetReferencePrefab(info.GetValue(nameof(Prefab), -1));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(StartEvent), StartEvent);
            info.AddValue(nameof(EndEvent), EndEvent);
            info.AddValue(nameof(Parent), Parent);
            info.AddValue(nameof(ActiveGameObject), ActiveGameObject != null ? ActiveGameObject.Metadata.SerializationId : -1);
            info.AddValue(nameof(Prefab), Prefab.PrefabId);
        }
    }
}
