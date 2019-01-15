using Newtonsoft.Json;

namespace PixelComrades {

    public class JsonDotNetSerializer : ISerializer {
        public Formatting Formatting = Formatting.Indented;
        public JsonSerializerSettings Settings = new JsonSerializerSettings();

        public string Serialize(object obj) {
            return JsonConvert.SerializeObject(obj, Formatting, Settings);
        }

        public T Deserialize<T>(string str) {
            return JsonConvert.DeserializeObject<T>(str, Settings);
        }
    }
}