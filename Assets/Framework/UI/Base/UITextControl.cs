using UnityEngine;
using System.Collections;
using TMPro;

namespace PixelComrades {
    public class UITextControl : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI _uguiText = null;
        [SerializeField] private TextMeshPro _normalText = null;

        public string text {
            get { return _uguiText != null ? _uguiText.text : _normalText.text; }
            set {
                if (_uguiText != null) {
                    _uguiText.text = value;
                }
                if (_normalText != null) {
                    _normalText.text = value;
                }
            }
        }

        public int maxVisibleCharacters {
            get { return _uguiText != null ? _uguiText.maxVisibleCharacters : _normalText.maxVisibleCharacters; }
            set {
                if (_uguiText != null) {
                    _uguiText.maxVisibleCharacters = value;
                }
                if (_normalText != null) {
                    _normalText.maxVisibleCharacters = value;
                }
            }
        }

        public void ClearTypewriter() {
            if (_normalText != null) {
                _normalText.maxVisibleCharacters = 9999;
            }
            if (_uguiText != null) {
                _uguiText.maxVisibleCharacters = 9999;
            }
        }

        public void MaxVisibleCharacters(int value) {
            if (_normalText != null) {
                _normalText.maxVisibleCharacters = value;
            }
            if (_uguiText != null) {
                _uguiText.maxVisibleCharacters = value;
            }
        }

        public void TypeWriterText(string displayText, float speed, bool unscaled, System.Action onComplete) {
            TimeManager.StartTask(DisplayText(displayText, speed), unscaled, onComplete);
        }

        private IEnumerator DisplayText(string textValue, float speed) {
            MaxVisibleCharacters(0);
            text = textValue;
            for (int i = 0; i < textValue.Length; i++) {
                maxVisibleCharacters++;
                yield return speed;
            }
        }
    }
}