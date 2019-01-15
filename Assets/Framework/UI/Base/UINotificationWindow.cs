using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace PixelComrades {
    public class UINotificationWindow : MonoSingleton<UINotificationWindow> {

        private enum State {
            Disabled,
            FadingOut,
            FadingIn,
            Enabled
        }

        //[SerializeField] private float _textUpdateSpeed = 0.2f;
        [SerializeField] private GameObject _msgPrefab = null;
        [SerializeField] private RectTransform _container = null;
        [SerializeField] private ScrollRect _scrollRect = null;
        [SerializeField] private CanvasGroup _canvasGroup = null;
        [SerializeField] private int _maxMessages = 15;
        [SerializeField] private float _msgTimeout = -1f;
        [SerializeField] private bool _fadeWindow = false;
        [SerializeField] private TweenFloat _fadeOutTween = null;
        [SerializeField] private TweenFloat _fadeInTween = null;

        private List<UINotificationMsg> _messages = new List<UINotificationMsg>();
        private State _state = State.Disabled;

        void Awake() {
            MessageKit<UINotificationWindow.Msg>.addObserver(Messages.MessageLog, AddNewMessage);
            MessageKit.addObserver(Messages.PlayerNewGame, StartNewGame);
        }

        private void StartNewGame() {
            for (int i = _messages.Count - 1; i >= 0; i--) {
                ItemPool.Despawn(_messages[i].gameObject);
            }
            _messages.Clear();
        }

        void Update() {
            if (!_fadeWindow || _msgTimeout <= 0) {
                return;
            }
            for (int i = _messages.Count - 1; i >= 0; i--) {
                if (TimeManager.TimeUnscaled > _messages[i].ExpireTime) {
                    _messages[i].Hide();
                    _messages.RemoveAt(i);
                }
            }
            if (_messages.Count == 0 && _state == State.Enabled) {
                //TimeManager.Start(FadeOut(), false);
                _state = State.FadingOut;
                _fadeOutTween.Restart(_canvasGroup.alpha, 0);
            }
            else if (_messages.Count > 0 && (_state == State.Disabled || _state == State.FadingOut)) {
                _state = State.FadingIn;
                _fadeInTween.Restart(_canvasGroup.alpha, 1);
            }
            if (_state == State.FadingOut) {
                _canvasGroup.alpha = _fadeOutTween.Get();
                if (!_fadeOutTween.Active) {
                    _canvasGroup.interactable = false;
                    _state = State.Disabled;
                }
            }
            if (_state == State.FadingIn) {
                _canvasGroup.alpha = _fadeInTween.Get();
                if (!_fadeInTween.Active) {
                    _canvasGroup.interactable = true;
                    _state = State.Enabled;
                }
            }
        }

        private void AddNewMessage(Msg message) {
            AddMessage(message.Text, message.Color);
        }

        public void AddMessage(string message, Color color) {
            if (string.IsNullOrEmpty(message)) {
                return;
            }
            //bool scrollbarAtBottom = false;
            //if (ScrollRect != null && ScrollRect.verticalScrollbar != null && ScrollRect.verticalScrollbar.value < 0.05f) {
            //    scrollbarAtBottom = true;
            //}
            var item = ItemPool.SpawnUIPrefab<UINotificationMsg>(_msgPrefab.gameObject, _container);
            //item.transform.SetSiblingIndex(0); // Move to the top of the list
            item.Show(message, color, 0);
            if (_scrollRect != null) {
                _scrollRect.verticalNormalizedPosition = 0.0f;
            }
            if (_msgTimeout > 0) {
                item.ExpireTime = TimeManager.TimeUnscaled + _msgTimeout;
            }
            _messages.Add(item);
            if (_messages.Count > _maxMessages) {
                _messages[0].Hide();
                _messages.RemoveAt(0);
            }
        }

        public struct Msg {
            public string Text;
            public Color Color;

            public Msg(string text, Color color) {
                Text = text;
                Color = color;
            }
        }
    }
}

