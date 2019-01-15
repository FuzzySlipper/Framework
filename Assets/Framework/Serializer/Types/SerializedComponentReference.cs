using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class SerializedComponentReference : SerializedGameObjectReference {

        private System.Type _type;
        private int _componentIndex;

        public override object GetValue() {
            var entity = GetWorldEntity();
            if (entity == null) {
                return null;
            }
            var components = entity.GetComponents(_type);
            if (components == null) {
                return null;
            }
            return components[MathEx.Min(_componentIndex, components.Length - 1)];
        }

        public SerializedComponentReference(PrefabEntity entity, Component component) : base(entity) {
            _type = component.GetType();
            var components = entity.GetComponents(_type);
            for (int i = 0; i < components.Length; i++) {
                if (components[i] == component) {
                    _componentIndex = i;
                    break;
                }
            }
        }

        public SerializedComponentReference(SerializationInfo info, StreamingContext context) : base(info, context) {
            _componentIndex = (int)info.GetValue("ComponentIndex", typeof(int));
            _type = (System.Type)info.GetValue("TargetType", typeof(System.Type));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue("ComponentIndex", _componentIndex, typeof(int));
            info.AddValue("TargetType", _type, typeof(System.Type));
        }
    }
}
