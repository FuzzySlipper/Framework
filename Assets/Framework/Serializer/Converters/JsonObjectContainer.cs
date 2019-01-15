using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public interface ISerializableObjectWrapper {
        /**
         * Return the contained value (actual type as defined by T).
         */
        object GetValue();
    }

    /**
     * Json.Net when deserializing arrays or child objects will not invoke custom converters
     * because Deserialize<T> is called with `object` as the type, not the correct type.  By
     * storing custom classes in container objects with strongly typed properties it is
     * possible to circumvent this restriction.
     */
    public class JsonObjectContainer<T> : ISerializable, ISerializableObjectWrapper {
        /// The value to be serialized.
        public T value;

        /**
         * Create a new container object with T type.
         */
        public JsonObjectContainer(T value) {
            this.value = value;
        }

        /**
         * Constructor coming from serialization.
         */
        public JsonObjectContainer(SerializationInfo info, StreamingContext context) {
            value = (T)info.GetValue("value", typeof(T));
        }

        /**
         * Serialize data for ISerializable.
         */
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("value", value, typeof(T));
        }

        /**
         * Get the contained value.
         */
        public object GetValue() {
            return value;
        }

        /**
         * Return the type contained within this wrapper.
         */
        public new System.Type GetType() {
            return typeof(T);
        }

        /**
         * Return the value stored in this container.
         */
        public static implicit operator T(JsonObjectContainer<T> container) {
            return container.value;
        }

        public override string ToString() {
            return "Container: " + value;
        }
    }
}
