using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixelComrades {
    public class UITooltip : MonoSingleton<UITooltip> {

        public static bool Active { get { return main._state != State.Disabled; } }
        public static bool CanActivate { get { return _canActivate; } set { _canActivate = value; } }
        private static bool _canActivate = true;

        [SerializeField, Range(0.01f, 1)] private float _fadeSpeed = 0.25f;
        [SerializeField, Range(0.1f, 4)] private float _waitLength = 1.25f;
        [SerializeField, Range(0.1f, 4)] private float _waitStartLength = 0.5f;
        [SerializeField] private Image _toolTipImage = null;
        [SerializeField] private TextMeshProUGUI _textTitle = null;
        [SerializeField] private TextMeshProUGUI _textDescr = null;
        [SerializeField] private EasingTypes _fadeOutEase = EasingTypes.CubicIn;
        [SerializeField] private EasingTypes _fadeInEase = EasingTypes.CubicOut;
        [SerializeField] private Image _compareToolTipImage = null;
        [SerializeField] private TextMeshProUGUI _compareTextTitle = null;
        [SerializeField] private TextMeshProUGUI _compareTextDescr = null;
        [SerializeField] private RectTransform _compareTr = null;

        private Vector2 _currentPivot;
        private HorizontalOffset _horizontal;
        private VerticalOffset _vertical;
        private RectTransform _rect;
        private CanvasGroup _canvasGroup;
        private State _state = State.Disabled;
        private TweenFloat _tween;
        private UnscaledTimer _waitTimer = new UnscaledTimer(1.25f);
        private Vector3[] _worldCorners = new Vector3[4];

        void Awake() {
            _rect = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        void Update() {
            if (_tween != null && _tween.Active) {
                _canvasGroup.alpha = _tween.Get();
            }
            switch (_state) {
                case State.Disabled:
                    return;
                case State.WaitingToDisable:
                    if (!_waitTimer.IsActive) {
                        _state = State.Disabled;
                        SetFadeTooltip(false);
                    }
                    break;
                case State.WaitingToShow:
                    if (!_waitTimer.IsActive) {
                        _state = State.Showing;
                        SetFadeTooltip(true);
                    }
                    break;
                //case State.Showing:
                //    //_rect.position = GetMouseRectPosition();
                //    AnchorPosition();
                //    LerpPosition();
                //    break;
            }
            if (Cursor.lockState == CursorLockMode.Locked || !PlayerInputSystem.IsCursorOverUI) {
                HideTooltip();
            }
        }

        private void SetFadeTooltip(bool active) {
            _tween = new TweenFloat(_canvasGroup.alpha, active ? 1 : 0, _fadeSpeed, active ? _fadeInEase : _fadeOutEase, true);
            _tween.Init();
        }

        public void HideTooltipImmediate() {
            if (UITooltipReplacer.Current != null) {
                UITooltipReplacer.Current.HideTooltip();
                return;
            }
            _state = State.Disabled;
            if (_tween != null) {
                _tween.Cancel();
            }
            _canvasGroup.alpha = 0;
        }

        public void HideTooltip() {
            if (UITooltipReplacer.Current != null) {
                UITooltipReplacer.Current.HideTooltip();
                return;
            }
            if (_state != State.Showing) {
                return;
            }
            _state = State.WaitingToDisable;
            _waitTimer.StartNewTime(_waitLength);
        }

        private void FadeTooltipIn() {
            if (_state == State.Showing || _state == State.WaitingToShow) {
                return;
            }
            if (_state == State.WaitingToDisable) {
                _state = State.Showing;
                SetFadeTooltip(true);
                return;
            }
            _state = State.WaitingToShow;
            _waitTimer.StartNewTime(_waitStartLength);
        }

        public void ShowToolTip(Image source, Sprite sprite, string title, string descr) {
            if (!_canActivate) {
                return;
            }
            if (UITooltipReplacer.Current != null) {
                UITooltipReplacer.Current.ShowToolTip(source, sprite, title, descr);
                return;
            }
            if (source == null) {
                return;
            }
            _toolTipImage.overrideSprite = sprite;
            _toolTipImage.enabled = sprite != null;
            _textTitle.text = title;
            _textDescr.text = descr;
            SetPivot(source.rectTransform.position);
            _rect.position = OffsetPosition(source.rectTransform);
            ShowToolTip();
        }

        public void ShowToolTip(RectTransform source, string title, string descr) {
            if (!_canActivate) {
                return;
            }
            if (UITooltipReplacer.Current != null) {
                UITooltipReplacer.Current.ShowToolTip(source, title, descr);
                return;
            }
            _textTitle.text = title;
            _textDescr.text = descr;
            _toolTipImage.enabled = false;
            SetPivot(source.position);
            _rect.position = OffsetPosition(source);
            ShowToolTip();
        }

        private void ShowToolTip() {
            //UICursor.main.SetCursor(_useCursor);
            _compareTr.gameObject.SetActive(false);
            SetImageActive(_toolTipImage.sprite != null);
            FadeTooltipIn();
        }

        public void ShowCompareToolTip(Sprite sprite = null, string title = "", string descr = "") {
            if (UITooltipReplacer.Current != null) {
                UITooltipReplacer.Current.ShowCompareToolTip(sprite, title, descr);
                return;
            }
            _compareTr.gameObject.SetActive(true);
            _compareToolTipImage.overrideSprite = sprite;
            _compareToolTipImage.gameObject.SetActive(_compareToolTipImage.sprite != null);
            _compareTextTitle.text = title;
            _compareTextDescr.text = descr;
            if (_horizontal == HorizontalOffset.Right) {
                var pivot = new Vector2(0, 1f);
                _compareTr.SetAnchors(pivot);
                _compareTr.pivot = pivot;
            }
            else {
                var pivot = new Vector2(1, 1f);
                _compareTr.SetAnchors(pivot);
                _compareTr.pivot = pivot;
            }
            _compareTr.anchoredPosition = _horizontal == HorizontalOffset.Right ? new Vector3(-325, 0, 0) : new Vector3(325, 0, 0);
        }

        private void SetPivot(Vector3 v) {
            if (v.x <= Screen.width / 2) {
                _horizontal = HorizontalOffset.Left;
            }
            else {
                _horizontal = HorizontalOffset.Right;
            }

            if (v.y > Screen.height / 2) {
                _vertical = VerticalOffset.Top;
            }
            else {
                _vertical = VerticalOffset.Botton;
            }

            if (_vertical == VerticalOffset.Top && _horizontal == HorizontalOffset.Left) {
                SetPivot(0, 1);
            }
            else if (_vertical == VerticalOffset.Top && _horizontal == HorizontalOffset.Right) {
                SetPivot(1, 1);
            }
            else if (_vertical == VerticalOffset.Botton && _horizontal == HorizontalOffset.Left) {
                SetPivot(0, 0);
            }
            else if (_vertical == VerticalOffset.Botton && _horizontal == HorizontalOffset.Right) {
                SetPivot(1, 0);
            }
        }

        private Vector3 OffsetPosition(RectTransform image) {
            image.GetWorldCorners(_worldCorners);
            if (_vertical == VerticalOffset.Botton && _horizontal == HorizontalOffset.Left) {
                return _worldCorners[2];
            }
            if (_vertical == VerticalOffset.Top && _horizontal == HorizontalOffset.Left) {
                return _worldCorners[3];
            }
            if (_vertical == VerticalOffset.Top && _horizontal == HorizontalOffset.Right) {
                return _worldCorners[0];
            }
            return _worldCorners[1];
        }

        //0 BL 1 TL 2 TR 3 BR

        private void SetPivot(float x, float y) {
            _currentPivot = new Vector2(x, y);
            _rect.pivot = _currentPivot;
        }

        private void SetImageActive(bool status) {
            _toolTipImage.enabled = status;
        }

        private enum VerticalOffset {
            Top,
            Botton,
        }

        private enum HorizontalOffset {
            Left,
            Right,
        }

        private enum State {
            WaitingToDisable,
            WaitingToShow,
            Showing,
            Disabled
        }
    }
}