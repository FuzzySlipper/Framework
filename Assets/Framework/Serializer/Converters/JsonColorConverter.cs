using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PixelComrades {
    public class JsonColorConverter : JsonConverter {

        public override bool CanConvert(Type objectType) {
            return typeof(Color).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var o = JObject.Load(reader);

            if (typeof(Color32).IsAssignableFrom(objectType)) {
                return new Color32((byte)o.GetValue("r"),
                    (byte)o.GetValue("g"),
                    (byte)o.GetValue("b"),
                    (byte)o.GetValue("a"));
            }

            return new Color((float)o.GetValue("r"),
                (float)o.GetValue("g"),
                (float)o.GetValue("b"),
                (float)o.GetValue("a"));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var o = new JObject();

            o.Add("$type", value.GetType().AssemblyQualifiedName);

            if (value is Color) {
                o.Add("r", ((Color)value).r);
                o.Add("g", ((Color)value).g);
                o.Add("b", ((Color)value).b);
                o.Add("a", ((Color)value).a);
            }
            else {
                o.Add("r", ((Color32)value).r);
                o.Add("g", ((Color32)value).g);
                o.Add("b", ((Color32)value).b);
                o.Add("a", ((Color32)value).a);
            }

            o.WriteTo(writer, serializer.Converters.ToArray());
        }

    }
}
