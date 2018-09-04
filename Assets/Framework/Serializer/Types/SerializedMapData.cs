using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class SerializedGenericData : ISerializable {

        public string Key;
        public string Data;

        public SerializedGenericData(SerializationInfo info, StreamingContext context) {
            Key = (string) info.GetValue("Key", typeof(string));
            Data = (string) info.GetValue("Data", typeof(string));
        }

        public SerializedGenericData(string key, string data) {
            Key = key;
            Data = data;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Key", Key, typeof(string));
            info.AddValue("Data", Data, typeof(string));
        }
    }

    public static class SerializedMapDataExtensiosn {
        public static SerializedGenericData FindData(this IList<SerializedGenericData> data, string key) {
            for (int i = 0; i < data.Count; i++) {
                if (key.CompareCaseInsensitive(data[i].Key)) {
                    return data[i];
                }
            }
            return null;
        }
    }
}
