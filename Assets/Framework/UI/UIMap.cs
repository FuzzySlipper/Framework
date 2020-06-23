using UnityEngine;
using System.Collections;


namespace PixelComrades {
    public class UIMap : MonoSingleton<UIMap>, IMenuControl {

        [SerializeField] private CanvasGroup _canvas = null;
        [SerializeField] private float _fadeLength = 0.75f;
        [SerializeField] private CanvasGroup _nodeCanvas = null;
        [SerializeField] private CanvasGroup _levelCanvas = null;
        [SerializeField] private CanvasGroup _minimapCanvas = null;

        private bool _isLevelCamera = true;
        private Task _animate;

        public bool IsLevelCamera { get { return _isLevelCamera; } }
        public bool Active { get; private set; }

        public void ToggleActive() {
            SetStatus(!Active);
        }

        public void SetMinimapStatus(bool status) {
            if (Active) {
                SetStatus(false);
            }
            _minimapCanvas.SetActive(status);
        }

        public void SetSceneStatus(bool status) {
            if (status == Active) {
                return;
            }
            gameObject.SetActive(status);
        }

        public void SetStatus(bool status) {
            if (status == Active || _animate != null) {
                return;
            }
            if (status) {
                Game.CursorUnlock("Map");
            }
            else {
                Game.RemoveCursorUnlock("Map");
            }
            //if (!status && RiftsMap.CanChangeCurrent) {
            //    RiftsMap.SetCanChange(false);
            //}
            Active = status;
            _animate = _canvas.FadeTo(status ? 1 : 0, _fadeLength, EasingTypes.SinusoidalInOut, true, Tween.TweenRepeat.Once, () => _animate = null);
            SetCamStatus(status);
        }

        public void SetInteractiveRiftMap() {
            if (Active) {
                SetStatus(false);
                return;
            }
            _isLevelCamera = false;
            //RiftsMap.SetCanChange(true);
            SetStatus(true);
        }

        private void SetCamStatus(bool status) {
            if (_isLevelCamera) {
                LevelMapCamera.main.SetStatus(status);
                _levelCanvas.SetActive(status);
            }
            else {
                _nodeCanvas.SetActive(status);
            }
        }

        public void ToggleCamera() {
            SetCamStatus(false);
            _isLevelCamera = !_isLevelCamera;
            SetCamStatus(true);
        }

        public void ToggleRiftView() {
            //RiftSpaceController.SetRiftSpaceStatus(!RiftSpaceController.Active);
        }
    }
}