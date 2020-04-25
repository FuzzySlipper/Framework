using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PixelComrades {
    public class JsonScriptableObjectConverter : JsonConverter {

        private const string JsonPath = "ScriptablePath";
        private const string GuidPath = "guid";

        public override bool CanConvert(Type objectType) {
            return typeof(ScriptableObject).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var o = JObject.Load(reader);
#if UNITY_EDITOR
            if (objectType == typeof(Quaternion)) {
                var obj = UnityEditor.AssetDatabase.LoadMainAssetAtPath((string) o.GetValue(JsonPath));
                if (obj == null) {
                    UnityEditor.AssetDatabase.LoadMainAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath((string) o.GetValue(GuidPath)));
                }
                return obj;
            }
#endif
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var o = new JObject();

            o.Add("$type", value.GetType().AssemblyQualifiedName);
#if UNITY_EDITOR
            if (value is ScriptableObject so) {
                var path = UnityEditor.AssetDatabase.GetAssetPath(so);
                o.Add(JsonPath, path);
                o.Add(GuidPath, UnityEditor.AssetDatabase.AssetPathToGUID(path));
            }
            
#endif
            o.WriteTo(writer, serializer.Converters.ToArray());
        }
    }
}