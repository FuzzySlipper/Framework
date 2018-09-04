using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIGenericDropdown : MonoBehaviour, IPoolEvents {

        [SerializeField] private Dropdown _dropdown = null;
        [SerializeField] private TextMeshProUGUI _text = null;

        public System.Action<int> OnValueChanged;
        public int Index { get; set; }
        public Dropdown Dropdown { get { return _dropdown; } }

        void Awake() {
            if (_dropdown == null) {
                _dropdown = GetComponent<Dropdown>();
            }
            if (_text == null) {
                _text = GetComponent<TextMeshProUGUI>();
            }
        }

        public void SetText(string text) {
            if (_text != null) {
                _text.text = text;
            }
        }

        public void ValueChanged(int index) {
            if (OnValueChanged != null) {
                OnValueChanged(index);
            }
        }

        public void OnPoolSpawned(){}

        public void OnPoolDespawned() {
            OnValueChanged = null;
        }
    }
}