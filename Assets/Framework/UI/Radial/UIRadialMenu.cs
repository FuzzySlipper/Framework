using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIRadialMenu : MonoBehaviour {
#pragma warning disable 649
        [Tooltip("How long do all the layers take to transition on to the screen by default")] [SerializeField] private float _transitionLength = 1.0f;
        [Tooltip("How big will elements scale when selected")] [SerializeField] private float _elementGrowthFactor = 1.5f;
        [Range(0.0f, 1.0f)] [Tooltip("Maximum size the element can be sized to as a percentage of the menu size")] [SerializeField] private float _elementMaxSize = 0.1f;
        [Tooltip("Does all of the layers have element snapping")] [SerializeField] private bool _elementSnapping;
        [Range(0.0f, 1.0f)] [Tooltip("Joystick axis deadzone")] [SerializeField] private float _inputDeadzone = 0.2f;
        [Tooltip("How far outside of the radial ring will input be recognised (0.1f is a good value)")] [SerializeField] private float _inputLeeway;
        [Tooltip("Should this radial menu react to joystick input")] [SerializeField] private bool _joystickInputEnabled;
        [Tooltip("Should this radial menu react to mouse input")] [SerializeField] private bool _mouseInputEnabled;
        [Range(0.0f, 1.0f)] [Tooltip("Radius of the circle that the elements are on as a percentage of the panel size")] [SerializeField] private float _panelOffset = 0.85f;

        [SerializeField] private TextMeshProUGUI _topText = null;
        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private UIRadialElement _radialPrefab = null;
        [SerializeField] private TweenFloat _fadeIn = new TweenFloat();
        [SerializeField] private TweenFloat _fadeOut = new TweenFloat();
        [SerializeField] private CanvasScaler _scaler = null;
        [SerializeField] private Transform _elementsPivot = null;
        [SerializeField] private TextMeshProUGUI _detailText = null;
        [SerializeField] private Image _detailSprite = null;
        [SerializeField] private Image _cursor = null;
        //[SerializeField] private Image _cursorIcon = null;
        //[SerializeField] private RectTransform _cursorIconHolder = null;
        [SerializeField] private Vector2 _positionLimit = Vector2.zero;
#pragma warning restore 649
        private string _defaultText;
        private State _state = State.Disabled;
        private UnscaledTimer _clickTimer = new UnscaledTimer(0.1f);
        private float _touchTimer;
        private ControlMethod _currentControl;
        private float _cursorTargetAngle;
        private Vector2 _oldMousePosition;
        private MenuActionLayer _currentLayer;
        private MenuActionLayer _nextLayer;
        private List<MenuActionLayer> _previousLayers = new List<MenuActionLayer>();
        
        private static UIRadialMenu _main;

        public static void Open(string defaultLabel, MenuActionLayer menuAction, Vector2 position) {
            _main.SetupRadial(defaultLabel, menuAction, position);
        }

        public static void Confirm(int index) {
            if (!Active) {
                return;
            }
            _main._currentLayer.Confirm(index);
        }

        public static bool Active { get { return _main != null && _main._state != State.Disabled; } }
        public static RectTransform PanelTransform { get; private set; }
        public float ScreenSize { get { return MathEx.Min(PanelTransform.rect.width, PanelTransform.rect.height) * _panelOffset / 2.0f; } }
        public float TransitionLength { get { return _transitionLength; } }
        public float ElementMaxSize { get { return _elementMaxSize; } }
        public bool ElementSnapping { get { return _elementSnapping; } }
        public float ElementGrowthFactor { get { return _elementGrowthFactor; } }
        public State CurrenState { get { return _state; } set { _state = value; } }
        //public Image CursorIcon { get { return _cursorIcon; } }
        public Image Cursor { get { return _cursor; } }
        public Transform ElementsParent { get { return _elementsPivot; } }
        public TextMeshProUGUI DetailText { get { return _detailText; } }
        public Image DetailSprite { get { return _detailSprite; } }
        public Vector2 ScreenScale {
            get {
                if ((byte)_scaler.uiScaleMode != 1) {
                    return Vector2.one;
                }
                return new Vector2(_scaler.referenceResolution.x / Screen.width, _scaler.referenceResolution.y / Screen.height);
            }
        }

        public static void Cancel() {
            if (Active) {
                _main.StartCloseRadial();
            }
        }

        public static void TransitionToNewLayer(MenuActionLayer actionLayer) {
            actionLayer.Init(_main, _main._cursorTargetAngle);
            _main._currentLayer.StartClose();
            _main._previousLayers.Add(_main._currentLayer);
            _main._nextLayer = actionLayer;
            _main._state = State.Changing;
        }

        void Awake() {
            _main = this;
            PanelTransform = transform as RectTransform;
        }

        void Update() {
            if (_state == State.Disabled || _state == State.Closing) {
                return;
            }
            switch (_state) {
                case State.Opening:
                    if (_fadeIn.Active) {
                        _canvasGroup.alpha = _fadeIn.Get();
                    }
                    if (_currentLayer.TransitionComplete() && !_fadeIn.Active) {
                        _state = State.Open;
                    }
                    break;
                case State.Changing:
                    if (_currentLayer.TransitionComplete() && _nextLayer.TransitionComplete()) {
                        _currentLayer = _nextLayer;
                        _state = State.Open;
                    }
                    break;
            }
            //hurry up is breaking animation fix later
            if (_currentLayer == null || _currentLayer.IsTransitioningOut || _state == State.Opening) {
                return;
            }
            _currentControl = ControlMethod.None;
            //TouchInput();
            ControllerInput();
            MouseInput();
        }



        public void SetupRadial(string defaultLabel, MenuActionLayer menuAction, Vector2 position) {
            if (_state == State.Opening || menuAction == null|| menuAction.Count == 0) {
                return;
            }
            UITooltip.main.HideTooltip();
            //UICursor.SetActive(false);
            _defaultText = defaultLabel;
            _topText.text = _defaultText;
            _detailText.text = "";
            TimeManager.StartUnscaled(OpenRadial(position, menuAction));
        }
        
        public IEnumerator OpenRadial(Vector2 startPosition, MenuActionLayer menuAction) {
            if (_state != State.Disabled) {
                StartCloseRadial();
                while (_state != State.Disabled) {
                    yield return null;
                }
            }
            _currentLayer = menuAction;
            PanelTransform.position = startPosition;
            var point = PanelTransform.anchoredPosition;
            point.x = Mathf.Clamp(point.x, -_positionLimit.x, _positionLimit.x);
            point.y = Mathf.Clamp(point.y, -_positionLimit.y, _positionLimit.y);
            PanelTransform.anchoredPosition = point;
            if (GameOptions.PauseForInput) {
                Game.Pause("Radial");
            }
            Game.CursorUnlock("Radial");
            _clickTimer.StartTimer();
            _state = State.Opening;
            DetermineTargetAngle(_currentControl);
            _currentLayer.Init(this, _cursorTargetAngle);
            _fadeIn.Restart(0, 1);
            _canvasGroup.SetActive(true);
        }

        public UIRadialElement CreateElement() {
            return ItemPool.SpawnUIPrefab<UIRadialElement>(_radialPrefab.gameObject, _elementsPivot);
        }

        public void StartCloseRadial() {
            TimeManager.StartUnscaled(CloseRadial());
        }

        public IEnumerator CloseRadial() {
            _state = State.Closing;
            _fadeOut.Restart(1, 0);
            if (_currentLayer != null) {
                _currentLayer.StartClose();
            }
            while (true) {
                if (_fadeOut.Active) {
                    _canvasGroup.alpha = _fadeOut.Get();
                }
                if (_currentLayer != null) {
                    if (_currentLayer.TransitionComplete() && !_fadeOut.Active) {
                        break;
                    }
                }
                else {
                    if (!_fadeOut.Active) {
                        break;
                    }
                }
                
                yield return null;
            }
            if (_currentLayer != null) {
                _currentLayer.Pool();
                _currentLayer = null;
            }
            for (int i = 0; i < _previousLayers.Count; i++) {
                _previousLayers[i].Pool();
            }
            _previousLayers.Clear();
            _canvasGroup.SetActive(false);
            if (GameOptions.PauseForInput) {
                Game.RemovePause("Radial");
            }
            Game.RemoveCursorUnlock("Radial");
            _state = State.Disabled;
        }

        public bool TransitionToPreviousLayer() {
            if (_previousLayers.Count == 0) {
                StartCloseRadial();
                return false;
            }
            var prev = _previousLayers.LastElement();
            _previousLayers.Remove(prev);
            _currentLayer.StartClose();
            _nextLayer = prev;
            _nextLayer.Init(this, _cursorTargetAngle);
            _state = State.Changing;
            _detailText.text = "";
            return true;
        }

        public void TestItemsEditor() {
            var panelTr = (RectTransform) transform;
            var screenSize = MathEx.Min(panelTr.rect.width, panelTr.rect.height) * _panelOffset / 2.0f;
            var radials = GetComponentsInChildren<UIRadialElement>();
            Vector2[] positions = new Vector2[radials.Length];
            var elementAngleDeg = 360.0f / radials.Length;
            var elementAngleRad = elementAngleDeg * Mathf.Deg2Rad;
            for (int i = 0; i < positions.Length; i++) {
                positions[i] = new Vector2();
                var tempAngle = i * elementAngleRad;
                positions[i].x = screenSize * Mathf.Sin(tempAngle);
                positions[i].y = screenSize * Mathf.Cos(tempAngle);
                radials[i].transform.localPosition = positions[i];
            }
        }

        private void MouseInput() {
            if (!_mouseInputEnabled || _currentControl != ControlMethod.None) {
                return;
            }
            _currentControl = ControlMethod.Mouse;
            if (_state == State.Open) {
                if (DetermineTargetAngle(ControlMethod.Mouse)) {
                    _currentLayer.UpdateLayer(_cursorTargetAngle, ControlMethod.Mouse);
                }
                if (!_clickTimer.IsActive && PlayerInputSystem.GetMouseButtonDown(0)) {
                    _clickTimer.StartTimer();
                    _currentLayer.Confirm();
                }
                if (!_clickTimer.IsActive && PlayerInputSystem.GetMouseButtonDown(1)) {
                    _clickTimer.StartTimer();
                    _currentLayer.Cancel();
                }
            }
            else if (!_clickTimer.IsActive && (PlayerInputSystem.GetMouseButtonDown(0) || PlayerInputSystem.GetMouseButtonDown(1))) {
                _clickTimer.StartTimer();
                _currentLayer.Skip();
            }
        }

        private void ControllerInput() {
            if (!_joystickInputEnabled|| _currentControl != ControlMethod.None) {
                return;
            }
            if (_state == State.Open) {
                if (DetermineTargetAngle(ControlMethod.Joystick)) {
                    _currentLayer.UpdateLayer(_cursorTargetAngle, ControlMethod.Joystick);
                }
                if (PlayerInputSystem.GetMouseButtonDown(0)) {
                    _currentControl = ControlMethod.Joystick;
                    _currentLayer.Confirm();
                }

                if (PlayerInputSystem.GetMouseButtonDown(1)) {
                    _currentControl = ControlMethod.Joystick;
                    _currentLayer.Cancel();
                }
            }
            else if (PlayerInputSystem.GetMouseButtonDown(0) || PlayerInputSystem.GetMouseButtonDown(1)) {
                _currentLayer.Skip();
            }
        }

        // private void TouchInput() {
        //     if (!_touchInputEnabled ||  _currentControl != ControlMethod.None) {
        //         return;
        //     }
        //     if (Input.touchCount <= 0) {
        //         _touchTimer = 0;
        //         return;
        //     }
        //     _currentControl = ControlMethod.Touch;
        //     var pointer = new PointerEventData(EventSystem.current);
        //     pointer.position = Input.GetTouch(0).position;
        //     var results = new List<RaycastResult>();
        //     EventSystem.current.RaycastAll(pointer, results);
        //     if (results.Count <= 0) {
        //         return;
        //     }
        //     var uiCoord = Input.GetTouch(0).position - (Vector2)PanelTransform.position;
        //     //if the touch is closer to the center of the menu than its radius
        //     //the radius of the menu is adjusted to compensate for different screen scalings
        //     float check = 1;
        //     switch ((byte)_scaler.uiScaleMode) {
        //         case 1:
        //             check = MathEx.Min(PanelTransform.rect.width, PanelTransform.rect.height) / Mathf.Lerp(ScreenScale.x, ScreenScale.y, _scaler.matchWidthOrHeight) * ((_panelOffset + _elementMaxSize) / 2.0f + _inputLeeway);
        //             break;
        //         case 2:
        //             check = MathEx.Min(PanelTransform.rect.width, PanelTransform.rect.height) * ((_panelOffset + _elementMaxSize) / 2.0f + _inputLeeway * 2.0f);
        //             break;
        //         default:
        //             check = MathEx.Min(PanelTransform.rect.width, PanelTransform.rect.height) * ((_panelOffset + _elementMaxSize) / 2.0f + _inputLeeway);
        //             break;
        //     }
        //     if (new Vector2(uiCoord.x, uiCoord.y).magnitude > check) {
        //         _touchTimer = 0;
        //         return;
        //     }
        //     if (_state != State.Open) {
        //         return;
        //     }
        //     //Save the mouse position to the observer variable
        //     _oldMousePosition = PlayerInputSystem.CursorPosition;
        //     //get the normalised form of the touch coordinates
        //     var uiDir = uiCoord.normalized;
        //     //Work out the clockwise angle from up to the UIDir
        //     var tempTargetAngle = Mathf.Atan2(uiDir.x, uiDir.y) * Mathf.Rad2Deg;
        //     //if the Direction is toward the left of the screen the angleneeds flipping
        //     if (uiCoord.x < 0.0f) {
        //         tempTargetAngle = 360.0f - Mathf.Abs(tempTargetAngle);
        //     }
        //
        //     var angleSet = false;
        //     //Check how to see how the touch should operate
        //     if (!_dragAndTouch) {
        //         _cursorTargetAngle = tempTargetAngle;
        //         angleSet = true;
        //     }
        //     //increment the touch timer by the time since the last frame
        //     _touchTimer += TimeManager.DeltaUnscaled;
        //     //if the touch timer is above the threshold
        //     if (_touchTimer >= _touchTime) {
        //         if (!angleSet) {
        //             _cursorTargetAngle = tempTargetAngle;
        //         }
        //
        //         //update the layer
        //         _currentLayer.UpdateLayer(_cursorTargetAngle, ControlMethod.Touch);
        //     }
        //     else {
        //         //if the touch let go this frame
        //         if (!_clickTimer.IsActive && Input.GetMouseButtonUp(0)) {
        //             _touchTimer = 0;
        //             _clickTimer.StartTimer();
        //             _currentLayer.UpdateLayer(_cursorTargetAngle, ControlMethod.Touch);
        //             _currentLayer.Confirm();
        //         }
        //     }
        // }

        private bool DetermineTargetAngle(ControlMethod method) {
            Vector2 input;
            switch (method) {
                default:
                case ControlMethod.Mouse:
                    if (_oldMousePosition == (Vector2) PlayerInputSystem.CursorPosition) {
                        return false;
                    }
                    _oldMousePosition = PlayerInputSystem.CursorPosition;
                    input = ((Vector2) PlayerInputSystem.CursorPosition - (Vector2) PanelTransform.position).normalized;
                    break;
                case ControlMethod.Joystick:
                    var horiz = PlayerInputSystem.LookInput.x;
                    var vert = PlayerInputSystem.LookInput.y;
                    if ((Mathf.Abs(horiz) < _inputDeadzone) && (Mathf.Abs(vert) < _inputDeadzone)) {
                        return false;
                    }
                    _currentControl = ControlMethod.Joystick;
                    input = new Vector2(horiz, vert).normalized;
                    break;
            }
            //work out the clockwise angle from up to the target direction
            var tempTargetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
            //if the target direction is to the left
            if (input.x < 0.0f) {
                tempTargetAngle = 360.0f - Mathf.Abs(tempTargetAngle);
            }
            _cursorTargetAngle = tempTargetAngle;
            return true;
        }

        public void SetCenterText(string text) {
            _topText.text = text;
        }

        public enum ControlMethod {
            None,
            Touch,
            Joystick,
            Mouse,
        }

        public enum State {
            Disabled,
            Opening,
            Changing,
            Open,
            Closing,
        }
    }
}