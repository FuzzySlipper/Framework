using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIGenericToggle : MonoBehaviour, IPoolEvents {

        [SerializeField] private Toggle _toggle = null;
        [SerializeField] private TextMeshProUGUI _text = null;

        public System.Action<bool> OnValueChanged;
        public int Index { get; set; }

        void Awake() {
            if (_toggle== null) {
                _toggle = GetComponent<Toggle>();
            }
            if (_text== null) {
                _text = GetComponent<TextMeshProUGUI>();
            }
        }
        
        public void SetInitialValue(bool status) {
            _toggle.isOn = status;
        }

        public void SetText(string text) {
            _text.text = text;
        }

        public void ValueChanged(bool value) {
            if (OnValueChanged != null) {
                OnValueChanged(_toggle.isOn);
            }
        }

        public void OnPoolSpawned() {
        }

        public void OnPoolDespawned() {
            OnValueChanged = null;
        }
    }
}