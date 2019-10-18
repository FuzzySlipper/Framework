using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class ActionFxComponent : IComponent {
        public ActionFx Value { get; private set; }

        public ActionFxComponent(ActionFx fx) {
            Value = fx;
        }

        public ActionFxComponent(SerializationInfo info, StreamingContext context) {
            ItemPool.LoadAsset<ActionFx>(info.GetValue(nameof(Value), ""), a => Value = a);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Value), ItemPool.GetAssetLocation(Value));
        }
        
        public void ChangeFx(ActionFx fx) {
            Value = fx;
        }
    }
}
