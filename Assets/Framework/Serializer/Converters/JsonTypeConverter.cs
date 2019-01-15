using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PixelComrades {
    public abstract class JsonTypeConverter<T> : JsonConverter {
        public sealed override bool CanConvert(Type objectType) {
            return typeof(T).IsAssignableFrom(objectType) || typeof(JsonObjectContainer<T>).IsAssignableFrom(objectType);
        }

        public sealed override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var o = JObject.Load(reader);
            var type = o.Property("$type");
            if (type != null) {
                var t = Type.GetType(type.Value.ToString());
                if (t == typeof(JsonObjectContainer<T>)) {
                    return ((JsonObjectContainer<T>)o.ToObject(typeof(JsonObjectContainer<T>), serializer)).value;
                }
            }
            return ReadJsonObject(o, objectType, existingValue, serializer);
        }

        /**
         * Behaves like ReadJson, which is overridden to handle conversion between wrapper and actual type.
         */
        public abstract object ReadJsonObject(JObject obj, Type objectType, object existingValue, JsonSerializer serializer);

        /**
         * Wrap `value` in a `JsonObjectContainer<T>` type in JSON.  Override `WriteObjectJson` to 
         * populate the value fields.
         */
        public sealed override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            writer.WriteStartObject();
            writer.WritePropertyName("$type");
            writer.WriteValue(typeof(JsonObjectContainer<T>).AssemblyQualifiedName);
            writer.WritePropertyName("value");

            WriteObjectJson(writer, value, serializer);

            writer.WriteEndObject();
        }

        /**
         * Behaves like WriteJson.
         */
        public abstract void WriteObjectJson(JsonWriter writer, object value, JsonSerializer serializer);

    }
}
