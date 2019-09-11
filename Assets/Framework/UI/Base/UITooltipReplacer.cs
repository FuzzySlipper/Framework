using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace PixelComrades {
    public class UITooltipReplacer : MonoBehaviour {

        public static UITooltipReplacer Current;

        [SerializeField] private Image _toolTipImage = null;
        [SerializeField] private TextMeshProUGUI _textTitle = null;
        [SerializeField] private TextMeshProUGUI _textDescr = null;
        [SerializeField] private Image _compareToolTipImage = null;
        [SerializeField] private TextMeshProUGUI _compareTextTitle = null;
        [SerializeField] private TextMeshProUGUI _compareTextDescr = null;
        [SerializeField] private RectTransform _compareTr = null;
        [SerializeField] private CanvasGroup _canvasGroup = null;

        public void SetActive(bool status) {
            if (status) {
                UITooltip.main.HideTooltipImmediate();
            }
            Current = status ? this : null;
        }

        public void HideTooltip() {
            _canvasGroup.alpha = 0;
        }

        public void ShowToolTip(Image source, Sprite sprite, string title, string descr) {
            _toolTipImage.overrideSprite = sprite;
            _toolTipImage.enabled = sprite != null;
            _textTitle.text = title;
            _textDescr.text = descr;
            ShowToolTip();
        }

        public void ShowToolTip(RectTransform source, string title, string descr) {
            _textTitle.text = title;
            _textDescr.text = descr;
            _toolTipImage.enabled = false;
            ShowToolTip();
        }

        public void ShowCompareToolTip(Sprite sprite = null, string title = "", string descr = "") {
            _compareTr.gameObject.SetActive(true);
            _compareToolTipImage.overrideSprite = sprite;
            _compareToolTipImage.gameObject.SetActive(_compareToolTipImage.sprite != null);
            _compareTextTitle.text = title;
            _compareTextDescr.text = descr;
        }

        private void ShowToolTip() {
            _compareTr.gameObject.SetActive(false);
            SetImageActive(_toolTipImage.sprite != null);
            _canvasGroup.alpha = 1;
        }

        private void SetImageActive(bool status) {
            _toolTipImage.gameObject.SetActive(status);
        }
    }
}
