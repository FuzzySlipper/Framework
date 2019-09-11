using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIDataDetailDisplay : MonoBehaviour {
        
        public static UIDataDetailDisplay Current;

        [SerializeField] private Image _toolTipImage = null;
        [SerializeField] private TextMeshProUGUI _textTitle = null;
        [SerializeField] private TextMeshProUGUI _textDescr = null;
        [SerializeField] private TextMeshProUGUI _textStats = null;
        [SerializeField] private Image _compareToolTipImage = null;
        [SerializeField] private TextMeshProUGUI _compareTextTitle = null;
        [SerializeField] private TextMeshProUGUI _compareDescr = null;
        [SerializeField] private TextMeshProUGUI _compareStats = null;
        [SerializeField] private RectTransform _compareTr = null;
        [SerializeField] private CanvasGroup _canvasGroup = null;

        private bool _active = false;

        public bool Active { get { return _active; } }

        public void SetActive(bool status) {
            Current = status ? this : null;
            _active = status;
            DisableCompare();
            ClearCurrentData();
            if (status) {
                UITooltip.main.HideTooltipImmediate();
            }
            if (_canvasGroup != null) {
                _canvasGroup.alpha = 0;
            }
            gameObject.SetActive(status);
        }

        public void HideTooltip() {
            if (_canvasGroup != null) {
                _canvasGroup.alpha = 0;
            }
        }

        public void Show(Sprite sprite, string title, string descr, string stats) {
            if (_toolTipImage != null) {
                _toolTipImage.overrideSprite = sprite;
                _toolTipImage.enabled = sprite != null;
            }
            _textTitle.text = title;
            _textDescr.text = descr;
            _textStats.text = stats;
            EnableDisplay();
        }

        public void Show(string title, string descr, string stats) {
            _textTitle.text = title;
            _textDescr.text = descr;
            _textStats.text = stats;
            if (_toolTipImage != null) {
                _toolTipImage.enabled = false;
            }
            EnableDisplay();
        }

        public void ShowCompare(Sprite sprite, string title, string descr, string stats) {
            _compareTr.gameObject.SetActive(true);
            _compareToolTipImage.overrideSprite = sprite;
            _compareToolTipImage.gameObject.SetActive(_compareToolTipImage.sprite != null);
            _compareTextTitle.text = title;
            _compareDescr.text = descr;
            _compareStats.text = stats;
        }

        protected void EnableDisplay() {
            DisableCompare();
            if (_toolTipImage != null) {
                _toolTipImage.gameObject.SetActive(_toolTipImage.sprite != null);
            }
            if (_canvasGroup != null) {
                _canvasGroup.alpha = 1;
            }
        }

        private void ClearCurrentData() {
            if (_toolTipImage != null) {
                _toolTipImage.overrideSprite = null;
                _toolTipImage.enabled = false;
            }
            _textTitle.text = "";
            _textDescr.text = "";
            _textStats.text = "";
        }

        private void DisableCompare() {
            if (_compareToolTipImage != null) {
                _compareToolTipImage.overrideSprite = null;
            }
            if (_compareTextTitle != null) {
                _compareTextTitle.text = "";
            }
            if (_compareDescr != null) {
                _compareDescr.text = "";
            }
            if (_compareStats != null) {
                _compareStats.text = "";
            }
            if (_compareTr != null) {
                _compareTr.gameObject.SetActive(false);
            }
        }
    }
}
