using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace PixelComrades {
    public class JsonDotNetBSONSerializer : ISerializer {
        public string Serialize(object obj) {
            MemoryStream ms = new MemoryStream();
            using (var writer = new BsonWriter(ms)) {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, obj);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public T Deserialize<T>(string str) {
            byte[] data = Convert.FromBase64String(str);
            MemoryStream ms = new MemoryStream(data);
            using (var reader = new BsonReader(ms)) {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }
    }
}