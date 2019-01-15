using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace PixelComrades {
    public class UIGenericValueWatcher : MonoBehaviour, ISystemUpdate, IOnCreate {

        public static UIGenericValueWatcher Get(UIAnchor anchor, float timer, Func<string> del) {
            var valueWatcher = ItemPool.SpawnUIPrefab<UIGenericValueWatcher>(UnityDirs.UI + "UIGenericValueWatcher", anchor.Get().transform);
            valueWatcher._timer.StartNewTime(timer);
            valueWatcher._checkTimer = timer > 0;
            valueWatcher._updateDel = del;
            anchor.SetAnchorPosition(valueWatcher._rectTr);
            return valueWatcher;
        }

        [SerializeField] private TextMeshProUGUI _text = null;

        private bool _checkTimer = false;
        private UnscaledTimer _timer = new UnscaledTimer();
        private Func<string> _updateDel;
        private RectTransform _rectTr;

        public void OnCreate(PrefabEntity entity) {
            _rectTr = transform as RectTransform;
            if (_text == null) {
                _text = GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        public void UpdateText() {
            if (_updateDel == null) {
                return;
            }
            _text.text = _updateDel();
        }

        public bool Unscaled { get { return true; } }

        public void OnSystemUpdate(float dt) {
            if (_checkTimer && !_timer.IsActive) {
                _timer.StartTimer();
                UpdateText();
            }
        }

        
    }
}
