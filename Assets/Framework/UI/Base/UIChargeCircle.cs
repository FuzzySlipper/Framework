using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIChargeCircle : MonoSingleton<UIChargeCircle> {

        [SerializeField] private Image _circle = null;
        [SerializeField] private Color _chargedColor = Color.green;
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private float _fadeTime = 0.35f;
        [SerializeField] private float _targetAlpha = 0.85f;
        [SerializeField] private TextMeshProUGUI _chargeText = null;
        [SerializeField] private CanvasGroup _canvasGroup = null;

        private Status _status = Status.Disabled;
        private float _startTime;

        private enum Status {
            Disabled,
            Charging,
            Fading
        }

        public static void ManualStart(string text) {
            main._chargeText.text = text;
            main._startTime = TimeManager.TimeUnscaled;
            ManualSetPercent(0);
        }

        public static void ManualSetPercent(float percent) {
            main.SetPercent(percent);
        }

        public static void StartCharge(float lerpTime) {
            TimeManager.StartUnscaled(main.Charge(lerpTime));
        }

        public static void StopCharge() {
            TimeManager.StartUnscaled(main.Fade());
        }

        private void SetPercent(float percent) {
            float runningTime = TimeManager.TimeUnscaled - _startTime;
            _canvasGroup.alpha = Mathf.Lerp(0, _targetAlpha, runningTime / _fadeTime);
            var color = Color.Lerp(_defaultColor, _chargedColor, percent);
            //color.a = Mathf.Lerp(0, _targetAlpha, runningTime / _fadeTime);
            _circle.color = color;
            _circle.fillAmount = percent;
        }

        private IEnumerator Fade() {
            var startTime = TimeManager.TimeUnscaled;
            var percent = 0f;
            var startAlpha = _circle.color.a;
            _status = Status.Fading;
            while (percent < 1 && _status != Status.Charging) {
                float runningTime = TimeManager.TimeUnscaled - startTime;
                percent = runningTime / _fadeTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, runningTime / _fadeTime);
                //var color = _circle.color;
                //color.a = Mathf.Lerp(startAlpha, 0, runningTime / _fadeTime);
                //_circle.color = color;
                yield return null;
            }
        }

        private IEnumerator Charge(float lerpTime) {
            _startTime = TimeManager.TimeUnscaled;
            var percent = 0f;
            _status = Status.Charging;
            while (percent < 1 && _status != Status.Fading) {
                float runningTime = TimeManager.TimeUnscaled - _startTime;
                percent = runningTime / lerpTime;
                _canvasGroup.alpha = Mathf.Lerp(0, _targetAlpha, runningTime / _fadeTime);
                var color = Color.Lerp(_defaultColor, _chargedColor, percent);
                //color.a = Mathf.Lerp(0, _targetAlpha, runningTime / _fadeTime);
                _circle.color = color;
                _circle.fillAmount = percent;
                yield return null;
            }
        }
    }
}