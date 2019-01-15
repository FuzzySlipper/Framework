using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PixelComrades {
    [IgnoreFileSerialization]
    public class SerializedLevelLoader : MonoBehaviour {
        [SerializeField] private TextAsset _level = null;

        public void LoadLevel() {
            transform.DeleteChildren();
            var scenegraph = JsonConvert.DeserializeObject<SerializedScene>(_level.text, Serializer.ConverterSettings);
            scenegraph.Restore(gameObject);
        }

        public void SaveLevel() {
            var rootNode = new SerializedScene(gameObject, gameObject.name);
            var scenegraph = JsonConvert.SerializeObject(rootNode, Formatting.Indented, Serializer.ConverterSettings);
            string path = "";
#if UNITY_EDITOR
            path = UnityEditor.AssetDatabase.GetAssetPath(_level);
#endif
            FileUtility.SaveFile(path, scenegraph);
        }
    }
}
