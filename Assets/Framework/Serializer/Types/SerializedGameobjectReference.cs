using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PixelComrades {
    public class SerializedGameObjectReference : ISerializable, ISerializableObjectWrapper {

        private string _path;
        private string _name;

        public string Name { get { return _name; } }

        public virtual object GetValue() {
            return GetWorldEntity();
        }

        public SerializedGameObjectReference(PrefabEntity entity) {
            
            _name = entity.name;
        }

        public SerializedGameObjectReference(SerializationInfo info, StreamingContext context) {
            _path = info.GetValue(nameof(_path), _path);
            _name = (string)info.GetValue("Name", typeof(string));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_path), _path);
            info.AddValue("Name", _name, typeof(string));
        }

        protected PrefabEntity GetWorldEntity() {
            return ItemPool.Spawn(_path);
        }
    }
}
