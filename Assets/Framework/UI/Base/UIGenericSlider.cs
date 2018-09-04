using System;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIGenericSlider : MonoBehaviour, IPoolEvents {

        [SerializeField] private Slider _slider = null;
        [SerializeField] private TextMeshProUGUI _text = null;

        private float _lastVal;

        public System.Action<UIGenericSlider, float> OnValueChanged;
        public int Index { get; set; }
        public Slider Slider { get { return _slider; } }

        void Awake() {
            if (_slider== null) {
                _slider = GetComponent<Slider>();
            }
            if (_text== null) {
                _text = GetComponent<TextMeshProUGUI>();
            }
        }

        public void SetNewSlider(float min, float max, float value, bool wholeNumbers = false) {
            _slider.minValue = min;
            _slider.maxValue = max;
            _slider.wholeNumbers = wholeNumbers;
            _slider.value = value;
            _lastVal = value;
            _text.text = _slider.wholeNumbers ? value.ToString("F0") : value.ToString("F1");
        }

        public void SetText(string text) {
            _text.text = text;
        }

        public void ValueChanged(float value) {
            _text.text = _slider.wholeNumbers ? value.ToString("F0") : value.ToString("F1");
            if (Math.Abs(value - _lastVal) < (_slider.wholeNumbers ? 0.95f : 0.11f)) {
                return;
            }
            _lastVal = value;
            if (OnValueChanged != null) {
                OnValueChanged(this, value);
            }
        }

        public void OnPoolSpawned() {
        }

        public void OnPoolDespawned() {
            OnValueChanged = null;
        }
    }
}