using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace PixelComrades {
    public class SerializedGameObjectReference : ISerializable, ISerializableObjectWrapper {

        private System.Type _db;
        private string _name;

        public virtual object GetValue() {
            return GetWorldEntity();
        }

        public SerializedGameObjectReference(PrefabEntity entity) {
            _name = entity.name;
            _db = entity.Db.GetType();
        }

        public SerializedGameObjectReference(SerializationInfo info, StreamingContext context) {
            _db = ParseUtilities.ParseType(info.GetValue(nameof(_db), ""));
            _name = info.GetValue(nameof(_name), _name);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_db), _db.ToString());
            info.AddValue(nameof(_name), _name);
        }

        protected PrefabEntity GetWorldEntity() {
            var db = ScriptableDatabases.GetDatabase(_db);
            var entity = db.GetObject<PrefabEntity>(_name);
            if (entity != null) {
                return ItemPool.Spawn(entity);
            }
            var go = db.GetObject<GameObject>(_name);
            return go != null ? ItemPool.Spawn(go) : null;
        }
    }
}