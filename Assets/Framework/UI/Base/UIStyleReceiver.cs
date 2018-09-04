using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIStyleReceiver : MonoBehaviour {

        [SerializeField] private UIStyle.ColorConfig _color = UIStyle.ColorConfig.InterfaceDark;
        [SerializeField] private UIStyle.TextConfig _textConfig = UIStyle.TextConfig.Standard;

        private Image _image;
        private TextMeshProUGUI _tmText;

        void Awake() {
            if (_color != UIStyle.ColorConfig.None) {
                _image = GetComponent<Image>();
                if (_image != null) {
                    _image.color = UIStyle.Get(_color);
                }
            }
            _tmText = GetComponent<TextMeshProUGUI>();
            if (_tmText != null) {
                if (_color != UIStyle.ColorConfig.None) {
                    _tmText.color = UIStyle.Get(_color);
                }
                if (_textConfig != UIStyle.TextConfig.None) {
                    _tmText.font = UIStyle.Get(_textConfig);
                }
            }
        }
    }
}