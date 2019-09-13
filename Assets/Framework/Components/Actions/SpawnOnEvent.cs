using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class SpawnOnEvent : IComponent, IReceive<ActionStateEvent> {

        public ActionStateEvents StartEvent { get; }
        public ActionStateEvents EndEvent { get; }
        public PrefabEntity Prefab { get; }
        public bool Parent { get; }

        private PrefabEntity _activeGameObject;

        public SpawnOnEvent(ActionStateEvents startEvent, ActionStateEvents endEvent, bool parent, PrefabEntity prefab) {
            StartEvent = startEvent;
            EndEvent = endEvent;
            Prefab = prefab;
            Parent = parent;
        }

        public void Handle(ActionStateEvent arg) {
            if (arg.State == EndEvent && _activeGameObject != null) {
                ItemPool.Despawn(_activeGameObject);
            }
            else if (arg.State == StartEvent && Prefab != null) {
                var animData = this.GetEntity().Find<AnimatorData>();
                var spawnPos = animData?.Animator?.GetEventPosition ?? (this.GetEntity().Tr != null ? this.GetEntity().Tr.position : Vector3.zero);
                var spawnRot = animData?.Animator?.GetEventRotation ?? (this.GetEntity().Tr != null ? this.GetEntity().Tr.rotation : Quaternion.identity);
                _activeGameObject = ItemPool.Spawn(Prefab, spawnPos, spawnRot);
                if (EndEvent == ActionStateEvents.None) {
                    _activeGameObject = null;
                }
            }
        }

        public SpawnOnEvent(SerializationInfo info, StreamingContext context) {
            StartEvent = info.GetValue(nameof(StartEvent), StartEvent);
            EndEvent = info.GetValue(nameof(EndEvent), EndEvent);
            Parent = info.GetValue(nameof(Parent), Parent);
            _activeGameObject = Serializer.GetPrefabEntity(info.GetValue(nameof(_activeGameObject), -1));
            Prefab = ItemPool.GetReferencePrefab(info.GetValue(nameof(Prefab), -1));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(StartEvent), StartEvent);
            info.AddValue(nameof(EndEvent), EndEvent);
            info.AddValue(nameof(Parent), Parent);
            info.AddValue(nameof(_activeGameObject), _activeGameObject != null ? _activeGameObject.Metadata.SerializationId : -1);
            info.AddValue(nameof(Prefab), Prefab.PrefabId);
        }
    }
}
