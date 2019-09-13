using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
	[System.Serializable]
	public sealed class PrefabComponent : IComponent {
        private CachedUnityComponent<PrefabEntity> _component;

        public PrefabComponent(PrefabEntity entity) {
            _component = new CachedUnityComponent<PrefabEntity>(entity);
        }

        public PrefabComponent(SerializationInfo info, StreamingContext context) {
            _component = info.GetValue(nameof(_component), _component);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_component), _component);
        }
    }
}
