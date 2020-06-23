using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class UIBasicMenu : MonoBehaviour, IMenuControl {

        [SerializeField] private CanvasGroup _canvasgroup = null;
        [SerializeField] private float _transitionLength = 0.35f;
        [SerializeField] private bool _registerOpenMenu = false;

        protected bool Status = false;
        private TweenFloat _fadeTween = null;
        private bool _lastStatusScene = false;
        private bool _status = false;

        public event System.Action OnWindowClosed;
        public static List<UIBasicMenu> OpenMenus = new List<UIBasicMenu>();
        protected float TransitionLength { get { return _transitionLength; } }
        protected CanvasGroup CanvasGroup{get { return _canvasgroup; }}
        public virtual bool Active { get { return _status; } }
        
        public virtual string GetTitleText() { return null; }

        protected virtual void OnStatusChanged(bool status) {
            if (!status && OnWindowClosed != null) {
                OnWindowClosed();
            }
            _status = status;
            if (!status) {
                if (_registerOpenMenu) {
                    OpenMenus.Remove(this);
                }
                UITooltip.main.HideTooltipImmediate();
            }
            else {
                if (_registerOpenMenu) {
                    OpenMenus.Add(this);
                }
            }
        }

        public void ToggleActive() {
            if (_lastStatusScene) {
                SetSceneStatus(!Active);
            }
            else {
                SetStatus(!Active);
            }
        }

        public virtual void SetSceneStatus(bool status) {
            if (status == Active) {
                return;
            }
            _lastStatusScene = true;
            gameObject.SetActive(status);
            if (_canvasgroup != null) {
                _canvasgroup.SetActive(status);
            }
            Status = status;
            OnStatusChanged(status);
        }

        public virtual void SetStatus(bool status) {
            if (status == Active || (_fadeTween != null && _fadeTween.Active)) {
                return;
            }
            ProcessSetStatus(status);
        }

        protected void ProcessSetStatus(bool status) {
            _lastStatusScene = false;
            if (_fadeTween == null) {
                _fadeTween = new TweenFloat(0, 1, _transitionLength, EasingTypes.SinusoidalInOut, true);
                _fadeTween.Init();
            }
            _canvasgroup.interactable = status;
            _canvasgroup.blocksRaycasts = status;
            _fadeTween.Restart(_canvasgroup.alpha, status ? 1 : 0);
            _fadeTween.Play((f => _canvasgroup.alpha = f), null);
            Status = status;
            OnStatusChanged(status);
        }

        public static void CloseAll() {
            for (int i = UIBasicMenu.OpenMenus.Count - 1; i >= 0; i--) {
                UIBasicMenu.OpenMenus[i].ToggleActive();
            }
        }
    }
}
