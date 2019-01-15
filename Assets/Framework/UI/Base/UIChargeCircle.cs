using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIChargeCircle : MonoSingleton<UIChargeCircle> {

        [SerializeField] private Image _circle = null;
        [SerializeField] private Color _chargedColor = Color.green;
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private float _fadeTime = 0.35f;
        [SerializeField] private float _targetAlpha = 0.85f;

        private Status _status = Status.Disabled;

        private enum Status {
            Disabled,
            Charging,
            Fading
        }

        public static void StartCharge(float lerpTime) {
            TimeManager.StartTask(main.Charge(lerpTime));
        }

        public static void StopCharge() {
            TimeManager.StartTask(main.Fade());
        }

        private IEnumerator Fade() {
            var startTime = TimeManager.Time;
            var percent = 0f;
            var startAlpha = _circle.color.a;
            _status = Status.Fading;
            while (percent < 1 && _status != Status.Charging) {
                float runningTime = TimeManager.Time - startTime;
                percent = runningTime / _fadeTime;
                var color = _circle.color;
                color.a = Mathf.Lerp(startAlpha, 0, runningTime / _fadeTime);
                _circle.color = color;
                yield return null;
            }
        }

        private IEnumerator Charge(float lerpTime) {
            var startTime = TimeManager.Time;
            var percent = 0f;
            _status = Status.Charging;
            while (percent < 1 && _status != Status.Fading) {
                float runningTime = TimeManager.Time - startTime;
                percent = runningTime / lerpTime;
                var color = Color.Lerp(_defaultColor, _chargedColor, percent);
                color.a = Mathf.Lerp(0, _targetAlpha, runningTime / _fadeTime);
                _circle.color = color;
                _circle.fillAmount = percent;
                yield return null;
            }
        }
    }
}