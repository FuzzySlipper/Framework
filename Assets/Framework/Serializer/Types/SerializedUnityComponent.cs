using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    //eg SerializableObject
    [Serializable]
    public class SerializedUnityComponent<T> : IUnitySerializable {

        /// A key-value store of all serializable properties and fields on this object.  Populated on serialization & deserialization.
        protected Dictionary<string, object> reflectedProperties;

        /// A reference to the component being serialized.  Will be null on deserialization.
        protected T target;

        public SerializedUnityComponent(T obj) {
            target = obj;
        }

        public SerializedUnityComponent(SerializationInfo info, StreamingContext context) {
            var typeName = (string)info.GetValue("typeName", typeof(string));
            Type = System.Type.GetType(typeName);
            reflectedProperties = (Dictionary<string, object>)info.GetValue("reflectedProperties", typeof(Dictionary<string, object>));
        }

        public Type Type { get; set; }

        /**
         * Serialize data for ISerializable.
         */
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            var type = target.GetType();
            info.AddValue("typeName", type.AssemblyQualifiedName, typeof(string));

            reflectedProperties = PopulateSerializableDictionary();
            info.AddValue("reflectedProperties", reflectedProperties, typeof(Dictionary<string, object>));
        }

        public virtual void ApplyProperties(object obj) {
            var ser = obj as ICustomSerializedUnityComponent;

            if (ser != null) {
                ser.ApplyDictionaryValues(reflectedProperties);
            }
            else {
                ReflectionHelper.ApplyProperties(obj, reflectedProperties);
            }
        }

        public virtual Dictionary<string, object> PopulateSerializableDictionary() {
            var ser = target as ICustomSerializedUnityComponent;

            if (ser != null) {
                return ser.PopulateSerializableDictionary();
            }
            return ReflectionHelper.ReflectProperties(target);
        }

        /**
         * Explicit cast return target.  If obj is null but reflectedProperties is valid, a new instance
         * of T is returned with those properties applied.  The new instance is constructed using default(T).
         */
        public static explicit operator T(SerializedUnityComponent<T> obj) {
            if (obj.target == null) {
                var val = default(T);
                obj.ApplyProperties(val);
                return val;
            }
            return obj.target;
        }
    }
}
