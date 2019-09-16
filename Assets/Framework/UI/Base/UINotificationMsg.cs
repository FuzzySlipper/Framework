using System;
using UnityEngine;
using System.Collections;
using PixelComrades;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public class UINotificationMsg : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler  {

        [SerializeField] private float _hideLength = 0.5f;
        [SerializeField] private TextMeshProUGUI _text = null;
        [SerializeField] private CanvasGroup _group = null;
        [SerializeField] private TweenFloat _hideTween = new TweenFloat();
        [SerializeField] private TweenFloat _revealTween = new TweenFloat();
        [SerializeField] private float _wordPause = 0.075f;
        [SerializeField] private Image _backgroundImage = null;

        private string _hoverMessage;
        
        public float ExpireTime { get; set; }

        private Task _currentTask;

        private IEnumerator RevealText(int length, float speed) {
            while (length > _text.maxVisibleCharacters) {
                _text.maxVisibleCharacters++;
                yield return null;
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            Game.DisplayData(_backgroundImage, null,_text.text, _hoverMessage, _hoverMessage);
        }

        public void OnPointerExit(PointerEventData eventData) {
            UITooltip.main.HideTooltip();
        }

        IEnumerator RevealWords(TMP_Text textComponent) {
            textComponent.ForceMeshUpdate();

            int totalWordCount = textComponent.textInfo.wordCount;
            int totalVisibleCharacters = textComponent.textInfo.characterCount; // Get # of Visible Character in text object
            int counter = 0;
            int currentWord = 0;
            int visibleCount = 0;

            while (visibleCount < totalVisibleCharacters) {
                currentWord = counter % (totalWordCount + 1);

                // Get last character index for the current word.
                if (currentWord == 0) // Display no words.
                {
                    visibleCount = 0;
                }
                else if (currentWord < totalWordCount) // Display all other words with the exception of the last one.
                {
                    visibleCount = textComponent.textInfo.wordInfo[currentWord - 1].lastCharacterIndex + 1;
                }
                else if (currentWord == totalWordCount) // Display last word and all remaining characters.
                {
                    visibleCount = totalVisibleCharacters;
                }

                textComponent.maxVisibleCharacters = visibleCount; // How many characters should TextMeshPro display?
                counter += 1;
                yield return _wordPause;
            }
        }

        private IEnumerator Despawn(float length) {
            _hideTween.Restart(_group.alpha, 0, length);
            while (_hideTween.Active) {
                _group.alpha = _hideTween.Get();
                yield return null;
            }
            ItemPool.Despawn(gameObject);
        }

        private IEnumerator FadeOut() {
            _hideTween.Restart(_group.alpha, 0, _hideLength);
            while (_hideTween.Active) {
                _group.alpha = _hideTween.Get();
                yield return null;
            }
        }

        private IEnumerator FadeIn() {
            _revealTween.Restart(0, 1);
            while (_revealTween.Active) {
                _group.alpha = _revealTween.Get();
                yield return null;
            }
        }

        public void Show(string message, string hover, Color color, float speed) {
            if (_currentTask != null) {
                TimeManager.Cancel(_currentTask);
            }
            if (_group == null) {
                Debug.LogErrorFormat("{0} has a null group {1}", name, _group == null);
            }
            else {
                _group.alpha = 0;
            }
            _text.color = color;
            _text.text = message;
            _text.maxVisibleCharacters = 0;
            _hoverMessage = hover;
            TimeManager.StartUnscaled(FadeIn());
            //TimeManager.StartUnscaled(RevealText(message.Length, speed), ()=> { _currentTask = null; });
            TimeManager.StartUnscaled(RevealWords(_text), () => { _currentTask = null; });
        }

        [UnityEngine.ContextMenu("Test FadeIn")] public void TestFadeIn() {
            TimeManager.StartUnscaled(FadeIn());
        }

        [UnityEngine.ContextMenu("Test FadeOut")] public void TestFadeOut() {
            TimeManager.StartUnscaled(FadeOut());
        }

        public void Hide() {
            TimeManager.StartUnscaled(Despawn(_hideLength));
        }
    }

    public class StaticNoticeMsg {
        private Color _messageColor = Color.green;
        private string _message;

        public StaticNoticeMsg(string message, Color color) {
            _message = message;
            _messageColor = color;
        }

        public StaticNoticeMsg(string message) {
            _message = message;
        }

        public void Show(params System.Object[] param) {
            string message;
            try {
                message = string.Format(_message, param);
            }
            catch (FormatException e) {
                Debug.LogFormat("Received {0} and {1} but threw {2}", _message, param.Length, e);
                return;
            }
            MessageKit<UINotificationWindow.Msg>.post(Messages.MessageLog, new UINotificationWindow.Msg(message, _messageColor));
        }

        public void ShowCenter(params System.Object[] param) {
            string message;
            try {
                message = string.Format(_message, param);
            }
            catch (FormatException e) {
                Debug.LogFormat("Received {0} and {1} but threw {2}", _message, param.Length, e);
                return;
            }
            UIFloatingText.SpawnCentered(message, _messageColor);
        }
    }
}