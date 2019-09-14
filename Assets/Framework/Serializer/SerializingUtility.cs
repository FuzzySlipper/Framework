using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    public static class SerializingUtility {

        public static void SaveJson(System.Object target, string path) {
            var save = JsonConvert.SerializeObject(target, Formatting.Indented, Serializer.ConverterSettings);
            FileUtility.SaveFile(path, save);
        }

        public static T LoadJson<T>(string path) where T : class {
            string text = FileUtility.ReadFile(path);
            if (string.IsNullOrEmpty(text)) {
                return null;
            }
            var data = JsonConvert.DeserializeObject<T>(text, Serializer.ConverterSettings);
            if (data == null) {
                Debug.LogErrorFormat("Error deserializing {0}", path);
                return null;
            }
            return data;
        }
    }
}
