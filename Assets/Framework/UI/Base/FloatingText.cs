using UnityEngine;
using System.Collections;
using TMPro;

namespace PixelComrades {
    public class FloatingText : MonoBehaviour {

        private static Vector3 _defaultEnd = new Vector3(0, 1f, 0);

        public static void Damage(float amount, Vector3 start) {
            FloatingTextHolder.Start(LazyDb.Main.FloatingTextDamage, amount.ToString("F0"), 4f, start, start + _defaultEnd, null);

        }

        public static void Message(string text, Vector3 start, Color color) {
            FloatingTextHolder.Start(LazyDb.Main.FloatingTextStandard, text, 1.5f, start, start + _defaultEnd, color);

        }

        public static void Message(string text, Vector3 start) {
            FloatingTextHolder.Start(LazyDb.Main.FloatingTextStandard, text, 1.5f,start,  start + _defaultEnd, null);
        }

        public static void Spawn(string text, float duration, Vector3 start, Vector3 end) {
            FloatingTextHolder.Start(LazyDb.Main.FloatingTextStandard, text, duration, start, end, null);
        }

        [SerializeField] private Color _defaultColor = Color.green;
        [SerializeField] private Color _endColor = Color.clear;
        [SerializeField] private TextMeshPro _text = null;
        [SerializeField] private TweenV3 _moveTween = new TweenV3();
        [SerializeField] private TweenFloat _colorTween = new TweenFloat();
        
        private class FloatingTextHolder : LoadOperationEvent {
            
            private static GenericPool<FloatingTextHolder> _pool = new GenericPool<FloatingTextHolder>(5);

            private FloatingText _floating;
            private string _text;
            private float _duration;
            private Vector3 _start;
            private Vector3 _end;
            private Color? _color;
            private ScaledTimer _timeoutTimer = new ScaledTimer(10);

            public static void Start(GameObjectReference prefab, string text, float duration, Vector3 start, Vector3 end, Color? color) {
                var txtHolder = _pool.New();
                txtHolder.Setup(prefab, text, duration, start, end, color);
            }

            private void Setup(GameObjectReference prefab, string text, float duration, Vector3 start, Vector3 end, Color? color) {
                SourcePrefab = prefab;
                _text = text;
                _start = start;
                _duration = duration;
                _end = end;
                _color = color;
            }

            public override void OnComplete() {
                _floating = NewPrefab.GetComponent<FloatingText>();
                TimeManager.StartTask(SetTargetText());
            }

            private IEnumerator SetTargetText() {
                var tr = _floating.transform;
                tr.position = _start;
                _floating._text.text = _text;
                _floating._text.color = _color ?? _floating._defaultColor;
                _floating._moveTween.Restart(tr.position, _end, _duration);
                _floating._colorTween.Restart(0, 1, _duration);
                _timeoutTimer.RestartTimer();
                while (_floating._moveTween.Active) {
                    _floating._text.color = Color.Lerp(_floating._defaultColor, _floating._endColor, _floating._colorTween.Get());
                    tr.position = _floating._moveTween.Get();
                    tr.LookAt(tr.position + Player.Cam.transform.rotation * Vector3.forward,PlayerCamera.Tr.rotation * Vector3.up);
                    //_text.fontSize = Vector3.Distance(transform.position, Player.Cam.transform.position) * 0.35f;
                    if (!_timeoutTimer.IsActive) {
                        break;
                    }
                    yield return null;
                }
                ItemPool.Despawn(_floating.gameObject);
                _floating = null;
                _color = null;
                SourcePrefab = null;
                NewPrefab = null;
                _pool.Store(this);
            }
        }
    }
}