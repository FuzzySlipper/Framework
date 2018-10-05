using UnityEngine;

namespace PixelComrades {
    public class UnityJsonSerializer : ISerializer {

        public bool PrettyPrint = false;
        
        public string Serialize(object obj) {
            return JsonUtility.ToJson(obj, PrettyPrint);
        }

        public T Deserialize<T>(string str) {
            return JsonUtility.FromJson<T>(str);
        }
    }
}