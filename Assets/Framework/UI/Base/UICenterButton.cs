using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace PixelComrades {
    public class UICenterButton : MonoSingleton<UICenterButton> {

        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private TextMeshProUGUI _text = null;

        private System.Action _clickDel;
        private ValueHolder<bool> _displaying = new ValueHolder<bool>(false);

        public static void Enable(string text, System.Action del, string id) {
            main.Setup(text, del, id);
        }

        public static void Disable(string id) {
            main.TryDisable(id);
        }

        public static bool TryClickEvent() {
            if (main._clickDel == null) {
                return false;
            }
            main.ClickEvent();
            return true;
        }

        public void ClickEvent() {
            if (_clickDel != null) {
                _clickDel();
            }
        }

        private void Setup(string text, System.Action del, string id) {
            _canvasGroup.SetActive(true);
            _text.text = text;
            _clickDel = del;
            _displaying.AddValue(true, id);
        }

        private void TryDisable(string id) {
            _displaying.RemoveValue(id);
            if (_displaying.Value) {
                return;
            }
            _canvasGroup.SetActive(false);
            _clickDel = null;
        }
    }
}
