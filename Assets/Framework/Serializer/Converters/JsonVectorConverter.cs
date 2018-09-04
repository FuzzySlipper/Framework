using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PixelComrades {
    public class JsonVectorConverter : JsonConverter {

        public override bool CanConvert(Type objectType) {
            return typeof(Vector2).IsAssignableFrom(objectType) || typeof(Quaternion).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var o = JObject.Load(reader);

            if (objectType == typeof(Vector2)) {
                return new Vector2((float)o.GetValue("x"),
                    (float)o.GetValue("y"));
            }

            if (objectType == typeof(Vector3)) {
                return new Vector3((float)o.GetValue("x"),
                    (float)o.GetValue("y"),
                    (float)o.GetValue("z"));
            }

            if (objectType == typeof(Vector4)) {
                return new Vector4((float)o.GetValue("x"),
                    (float)o.GetValue("y"),
                    (float)o.GetValue("z"),
                    (float)o.GetValue("w"));
            }

            if (objectType == typeof(Quaternion)) {
                return new Quaternion((float)o.GetValue("x"),
                    (float)o.GetValue("y"),
                    (float)o.GetValue("z"),
                    (float)o.GetValue("w"));
            }

            return new Vector3(0, 0, 0);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var o = new JObject();

            o.Add("$type", value.GetType().AssemblyQualifiedName);

            if (value is Vector2) {
                o.Add("x", ((Vector2)value).x);
                o.Add("y", ((Vector2)value).y);
            }
            else if (value is Vector3) {
                o.Add("x", ((Vector3)value).x);
                o.Add("y", ((Vector3)value).y);
                o.Add("z", ((Vector3)value).z);
            }
            else if (value is Vector4) {
                o.Add("x", ((Vector4)value).x);
                o.Add("y", ((Vector4)value).y);
                o.Add("z", ((Vector4)value).z);
                o.Add("w", ((Vector4)value).w);
            }
            else if (value is Quaternion) {
                o.Add("x", ((Quaternion)value).x);
                o.Add("y", ((Quaternion)value).y);
                o.Add("z", ((Quaternion)value).z);
                o.Add("w", ((Quaternion)value).w);
            }

            o.WriteTo(writer, serializer.Converters.ToArray());
        }
    }
}
