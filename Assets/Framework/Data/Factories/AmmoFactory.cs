using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class AmmoFactory : MonoBehaviour {
        private static Dictionary<string, AmmoConfig> _templates = new Dictionary<string, AmmoConfig>();

        private static void Init() {
            GameData.AddInit(Init);
            foreach (var loadedDataEntry in GameData.GetSheet("Ammo")) {
                var data = loadedDataEntry.Value;
                _templates.AddOrUpdate(data.ID, new AmmoConfig(data));
            }
        }

        public static AmmoConfig GetTemplate(string name) {
            if (_templates.Count == 0) {
                Init();
            }
            if (_templates.TryGetValue(name, out var data)) {
                return data;
            }
            Debug.LogFormat("No ammo {0}", name);
            return null;
        }

        public static AmmoConfig GetTemplate(DataReference dRef) {
            if (_templates.Count == 0) {
                Init();
            }
            if (_templates.TryGetValue(dRef.Value.ID, out var data)) {
                return data;
            }
            if (_templates.TryGetValue(dRef.Value.FullID, out data)) {
                return data;
            }
            Debug.LogFormat("No ammo {0}", dRef.TargetID);
            return null;
        }
    }

    public class AmmoConfig {
        public string ID;
        public string Name;
        public string ReloadText;
        public float ReloadSpeed;
        public List<KeyValuePair<string, float>> Cost = new List<KeyValuePair<string, float>>();

        public AmmoConfig(DataEntry data) {
            ID = data.ID;
            Name = data.TryGetValue(DatabaseFields.Name, data.ID);
            ReloadText = data.TryGetValue("ReloadText", data.ID);
            ReloadSpeed = data.TryGetValue("ReloadSpeed", 1f);
            AddToList(Cost, "id", "Amount", data.Get<DataList>("Currency"));
        }

        private void AddToList(List<KeyValuePair<string, float>> list, string keyString, string keyFloat, DataList data) {
            if (data == null) {
                return;
            }
            for (int i = 0; i < data.Count; i++) {
                var line = data[i];
                list.Add(new KeyValuePair<string, float>(line.GetValue<string>(keyString), line.GetValue<float>(keyFloat)));
            }
        }
    }
}
