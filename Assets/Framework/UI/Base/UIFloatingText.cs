using UnityEngine;
using System.Collections;
using PixelComrades;
using TMPro;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIFloatingText : MonoBehaviour {

        private static MsgChecker _msgChecker = new MsgChecker(20, 0.2f);

        public static void Spawn(string text, float duration, RectTransform start, Color startColor) {
            Spawn(text, duration, start, start.position + new Vector3(0, 50, 0), startColor);
        }

        public static void SpawnCentered(string text, Color startColor) {
            Spawn(text, 4, UIPlayerComponents.CenterMessage, startColor);
        }

        public static void SpawnCentered(string text, float duration, Color startColor) {
            Spawn(text, duration, UIPlayerComponents.CenterMessage, startColor);
        }

        public static void InventoryMessage(string text, RectTransform start) {
            if (!GameOptions.VerboseInventory || string.IsNullOrEmpty(text)) {
                return;
            }
            Spawn(text, 2f, start, start.position + new Vector3(0, 50, 0), Color.green);
        }

        public static void SpawnIcon(Sprite icon, string text, RectTransform start, Color color) {
            var startPosition = start.position;
            if (_iconTimer.IsActive) {
                startPosition = start.position + new Vector3(_negative ? -Offset : Offset, 0, 0);
                _negative = !_negative;
            }
            _iconTimer.StartTimer();
            FloatingTextHolder.Start(Orientation.Icon, text, 2f, startPosition, startPosition + new Vector3(0, 150, 0), UIRoot.Misc.Get()
            .transform, color, icon);

        }

        private static bool _negative;
        private static UnscaledTimer _iconTimer = new UnscaledTimer(0.5f);
        private const float Offset = 150f;

        public static void Spawn(string text, RectTransform start, Color color, Orientation orientation) {
            if (string.IsNullOrEmpty(text)) {
                return;
            }
            FloatingTextHolder.Start(orientation, text, 2f, start.position, start.position + new Vector3(0, 150, 0), UIRoot.Player.Get().transform, color);
        }

        public static void WorldSpawn(string text, Vector3 start, Color color) {
            if (string.IsNullOrEmpty(text)) {
                return;
            }
            _msgChecker.Add(text, UIRoot.Misc.Get().transform);
            start = RectTransformUtility.WorldToScreenPoint(Player.Cam, start);
            var end = RectTransformUtility.WorldToScreenPoint(Player.Cam, start + new Vector3(0, 1, 0));
            FloatingTextHolder.Start(Orientation.Center, text, 2f, start, end, UIRoot.Misc.Get().transform, color);

        }

        public static void Spawn(string text, float duration, RectTransform start, Vector3 end, Color startColor) {
            if (string.IsNullOrEmpty(text) || _msgChecker.ContainsDuplicate(text, start)) {
                return;
            }
            _msgChecker.Add(text, start);
            FloatingTextHolder.Start(Orientation.Center, text, duration, start.position, end, UIRoot.Player.Get().transform, startColor);

        }

        [SerializeField] private Color _endColor = Color.clear;
        [SerializeField] private TextMeshProUGUI _text = null;
        [SerializeField] private TweenV3 _moveTween = new TweenV3();
        [SerializeField] private TweenFloat _colorTween = new TweenFloat();
        [SerializeField] private Image _icon = null;

        public RectTransform RectTransform { get; private set; }

        void Awake() {
            RectTransform = GetComponent<RectTransform>();
        }

        public enum Orientation {
            Center,
            Right,
            Left,
            Icon,
        }

        private class FloatingTextHolder : LoadOperationEvent {
            
            private static GenericPool<FloatingTextHolder> _pool = new GenericPool<FloatingTextHolder>(5);

            private UIFloatingText _floating;
            private string _text;
            private float _duration;
            private Vector3 _start;
            private Vector3 _end;
            private Color _startColor;
            private Transform _parent;
            private Sprite _icon;
            private ScaledTimer _timeoutTimer = new ScaledTimer(10);

            public static void Start(Orientation type, string text, float duration, Vector3 start, Vector3 end, Transform parent, Color 
            color, Sprite icon = null) {
                var txtHolder = _pool.New();
                txtHolder.Setup(LazyDb.Main.UIFloatingText[(int) type], text, duration, start, end, parent, color, icon);
            }

            private void Setup(GameObjectReference prefab, string text, float duration, Vector3 start, Vector3 end, Transform parent, Color color, Sprite icon) {
                SourcePrefab = prefab;
                _text = text;
                _start = start;
                _duration = duration;
                _end = end;
                _startColor = color;
                _parent = parent;
                _icon = icon;
            }

            public override void OnComplete() {
                _floating = NewPrefab.GetComponent<UIFloatingText>();
                if (_icon != null) {
                    _floating._icon.overrideSprite = _icon;
                }
                _floating.RectTransform.SetParent(_parent);
                _floating.RectTransform.SetAsLastSibling();
                TimeManager.StartTask(SetTargetText());
            }

            private IEnumerator SetTargetText() {
                var tr = _floating.transform;
                tr.position = _start;
                _floating._text.text = _text;
                _floating._moveTween.Restart(tr.position, _end, _duration);
                _floating._colorTween.Restart(0, 1, _duration);
                _timeoutTimer.RestartTimer();
                while (_floating._moveTween.Active) {
                    _floating._text.color = Color.Lerp(_startColor, _floating._endColor, _floating._colorTween.Get());
                    tr.position = _floating._moveTween.Get();
                    yield return null;
                }
                ItemPool.Despawn(_floating.gameObject);
                _floating = null;
                _parent = null;
                _icon = null;
                SourcePrefab = null;
                NewPrefab = null;
                _pool.Store(this);
            }
        }

        private class MsgChecker {
            private float _minDuplicateTime;
            private MsgEntry[] _msgEntry;
            private int _index = 0;

            public MsgChecker(int maxSize, float dupeTime) {
                _minDuplicateTime = dupeTime;
                _msgEntry = new MsgEntry[maxSize];
                for (int i = 0; i < _msgEntry.Length; i++) {
                    _msgEntry[i] = new MsgEntry();
                }
            }

            public void Add(string text, Transform target) {
                _msgEntry[_index].Text = text;
                _msgEntry[_index].Target = target;
                _msgEntry[_index].Time = TimeManager.Time;
                _index++;
                if (_index >= _msgEntry.Length) {
                    _index = 0;
                }
            }

            public bool ContainsDuplicate(string text, Transform target) {
                for (int i = 0; i < _msgEntry.Length; i++) {
                    if (_msgEntry[i].Text.Length != text.Length) {
                        continue;
                    }
                    if (_msgEntry[i].Text != text || _msgEntry[i].Target != target) {
                        continue;
                    }
                    if (TimeManager.Time - _msgEntry[i].Time < _minDuplicateTime) {
                        return true;
                    }
                }
                return false;
            }

            private class MsgEntry {
                public string Text = "";
                public Transform Target;
                public float Time = 0;
            }
        }
    }
}