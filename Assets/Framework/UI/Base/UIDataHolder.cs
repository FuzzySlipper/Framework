using UnityEngine;
using System.Collections;

namespace PixelComrades {
    [System.Serializable]
    public class UIDataHolder {

        public void Spawn(string title, Transform parent, bool isV3) {
            if (isV3) {
                _text = ItemPool.SpawnUIPrefab<UIDataText>(string.Format("{0}DataTextV3", StringConst.PathUI), parent);
            }
            else {
                _text = ItemPool.SpawnUIPrefab<UIDataText>(string.Format("{0}DataText", StringConst.PathUI), parent);
            }
            _text.SetTitle(title);
        }

        public delegate float GetFloat();

        public delegate int GetInt();

        public delegate Vector3 GetV3();

        public GetFloat FloatUpdate = null;
        public GetInt IntUpdate = null;
        public GetV3 V3Update = null;

        private UIDataText _text;

        public void Update() {
            if (FloatUpdate != null) {
                _text.UpdateData(FloatUpdate());
            }
            if (IntUpdate != null) {
                _text.UpdateData(IntUpdate());
            }
            if (V3Update != null) {
                _text.UpdateData(V3Update());
            }
        }

        public void Reset() {
            FloatUpdate = null;
            IntUpdate = null;
            V3Update = null;
            ItemPool.Despawn(_text.gameObject);
        }

        public void UpdateData(string data) {
            _text.StartWritingData(data);
        }
    }
}