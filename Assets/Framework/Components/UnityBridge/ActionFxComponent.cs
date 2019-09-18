using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class ActionFxComponent : IComponent {
        public ActionFx Fx { get; private set; }

        public ActionFxComponent(ActionFx fx) {
            Fx = fx;
        }

        public ActionFxComponent(SerializationInfo info, StreamingContext context) {
            Fx = ItemPool.LoadAsset<ActionFx>(info.GetValue(nameof(Fx), ""));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Fx), ItemPool.GetAssetLocation(Fx));
        }
        
        public void ChangeFx(ActionFx fx) {
            Fx = fx;
        }
    }
}
