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
            UIFloatingText spawn = ItemPool.SpawnUIPrefab<UIFloatingText>("UI/UIFloatingTextIcon", start);
            spawn.RectTransform.SetParent(UIRoot.Misc.Get().transform);
            spawn.RectTransform.SetAsLastSibling();
            if (_iconTimer.IsActive) {
                startPosition = start.position + new Vector3(_negative ? -Offset : Offset, 0, 0);
                spawn.RectTransform.position = startPosition;
                _negative = !_negative;
            }
            _iconTimer.StartTimer();
            spawn._icon.sprite = icon;
            TimeManager.StartUnscaled(spawn.SetTargetText(text, 2f, startPosition + new Vector3(0, 150, 0), color));
        }

        private static bool _negative;
        private static UnscaledTimer _iconTimer = new UnscaledTimer(0.5f);
        private const float Offset = 150f;

        public static UIFloatingText Spawn(string text, RectTransform start, Color color, Orietation orientation) {
            if (string.IsNullOrEmpty(text)) {
                return null;
            }
            UIFloatingText spawn;
            switch (orientation) {
                case Orietation.Right:
                    spawn = ItemPool.SpawnUIPrefab<UIFloatingText>("UI/UIFloatingTextRight", start);
                    break;
                case Orietation.Left:
                    spawn = ItemPool.SpawnUIPrefab<UIFloatingText>("UI/UIFloatingTextLeft", start);
                    break;
                case Orietation.Icon:
                    spawn = ItemPool.SpawnUIPrefab<UIFloatingText>("UI/UIFloatingTextIcon", start);
                    break;
                default:
                    spawn = ItemPool.SpawnUIPrefab<UIFloatingText>("UI/UIFloatingTextStandard", start);
                    break;
            }
            //spawn.RectTransform.sizeDelta = start.sizeDelta;
            spawn.RectTransform.SetParent(UIRoot.Player.Get().transform);
            spawn.RectTransform.SetAsLastSibling();
            TimeManager.StartUnscaled(spawn.SetTargetText(text, 2f, start.position + new Vector3(0, 150, 0), color));
            return spawn;
        }

        public static void WorldSpawn(string text, Vector3 start, Color color) {
            if (string.IsNullOrEmpty(text)) {
                return;
            }
            _msgChecker.Add(text, UIRoot.Misc.Get().transform);
            UIFloatingText spawn = ItemPool.SpawnUIPrefab<UIFloatingText>("UI/UIFloatingTextStandard", UIRoot.Misc.Get().transform);
            spawn.transform.position = RectTransformUtility.WorldToScreenPoint(Player.Cam, start);
            var end = RectTransformUtility.WorldToScreenPoint(Player.Cam, start + new Vector3(0, 1, 0));
            spawn.RectTransform.SetAsLastSibling();
            TimeManager.StartUnscaled(spawn.SetTargetText(text, 2f, end, color));
        }

        public static void Spawn(string text, float duration, RectTransform start, Vector3 end, Color startColor) {
            if (string.IsNullOrEmpty(text) || _msgChecker.ContainsDuplicate(text, start)) {
                return;
            }
            _msgChecker.Add(text, start);
            var spawn = ItemPool.SpawnUIPrefab<UIFloatingText>("UI/UIFloatingTextStandard", start);
            spawn.RectTransform.SetParent(UIRoot.Player.Get().transform);
            spawn.RectTransform.SetAsLastSibling();
            TimeManager.StartUnscaled(spawn.SetTargetText(text, duration, end, startColor));
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

        public IEnumerator SetTargetText(string text, float duration, Vector3 end, Color start) {
            _text.text = text;
            _text.color = start;
            _moveTween.Restart(transform.position, end, duration);
            _colorTween.Restart(0, 1, duration);
            while (_moveTween.Active) {
                _text.color = Color.Lerp(start, _endColor, _colorTween.Get());
                transform.position = _moveTween.Get();
                yield return null;
            }
            ItemPool.Despawn(gameObject);
        }

        public enum Orietation {
            Center,
            Right,
            Left,
            Icon,
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