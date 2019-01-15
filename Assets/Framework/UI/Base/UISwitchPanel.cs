using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class UISwitchPanel : MonoBehaviour {

        [SerializeField] private CanvasGroup[] _canvasGroups = new CanvasGroup[0];

        private int _currentIndex = 0;

        public void Switch(int index) {
            DisableCurrent();
            _currentIndex = index;
            EnableCurrent();
        }

        private void DisableCurrent() {
            if (_canvasGroups.HasIndex(_currentIndex)) {
                _canvasGroups[_currentIndex].SetActive(false);
            }
        }

        private void EnableCurrent() {
            if (_canvasGroups.HasIndex(_currentIndex)) {
                _canvasGroups[_currentIndex].SetActive(true);
            }
        }

        public void AdvancePanel() {
            DisableCurrent();
            _currentIndex++;
            if (!_canvasGroups.HasIndex(_currentIndex)) {
                _currentIndex = 0;
            }
            EnableCurrent();
        }
    }
}
