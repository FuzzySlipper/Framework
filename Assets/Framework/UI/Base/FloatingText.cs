using UnityEngine;
using System.Collections;
using TMPro;

namespace PixelComrades {
    public class FloatingText : MonoBehaviour {

        private static Vector3 DefaultEnd = new Vector3(0, 1f, 0);

        public static void Spawn(string text, float duration, Vector3 start) {
            Spawn(text, duration, start, start + DefaultEnd);
        }

        public static void Damage(float amount, Vector3 start) {
            var spawn = ItemPool.Spawn<FloatingText>("UI/FloatingTextDamage", start, Quaternion.identity, true, false);
            spawn.StartText(amount.ToString("F0"), 4f, start + DefaultEnd);
        }

        public static void Message(string message, Vector3 start, Color color) {
            var spawn = ItemPool.Spawn<FloatingText>("UI/FloatingTextStandard", start, Quaternion.identity, true, false);
            spawn.StartText(message, 4f, start + DefaultEnd, color);
        }

        public static void Message(string message, Vector3 start) {
            var spawn = ItemPool.Spawn<FloatingText>("UI/FloatingTextStandard", start, Quaternion.identity, true, false);
            spawn.StartText(message, 4f, start + DefaultEnd);
        }

        public static void Spawn(string text, float duration, Vector3 start, Vector3 end) {
            var spawn = ItemPool.Spawn<FloatingText>("UI/FloatingTextStandard", start, Quaternion.identity, true, false);
            spawn.StartText(text, duration, end);
        }

        [SerializeField] private Color _defaultColor = Color.green;
        [SerializeField] private Color _endColor = Color.clear;
        [SerializeField] private TextMeshPro _text = null;
        [SerializeField] private TweenV3 _moveTween = new TweenV3();
        [SerializeField] private TweenFloat _colorTween = new TweenFloat();

        public void StartText(string text, float duration, Vector3 end) {
            StartText(text, duration, end, _endColor);
        }

        public void StartText(string text, float duration, Vector3 end, Color color) {
            TimeManager.StartTask(SetTargetText(text, duration, end, color));
        }

        public IEnumerator SetTargetText(string text, float duration, Vector3 end, Color color) {
            _text.text = text;
            _text.color = _defaultColor;
            _moveTween.Restart(transform.position, end, duration);
            _colorTween.Restart(0, 1, duration);
            while (_moveTween.Active) {
                _text.color = Color.Lerp(_defaultColor, _endColor, _colorTween.Get());
                transform.position = _moveTween.Get();
                transform.LookAt(transform.position + Player.Cam.transform.rotation * Vector3.forward,
                    Player.Cam.transform.rotation * Vector3.up);
                _text.fontSize = Vector3.Distance(transform.position, Player.Cam.transform.position) * 0.35f;
                yield return null;
            }
            ItemPool.Despawn(gameObject);
        }
    }
}