using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace PixelComrades {
    public class UICenterTarget : MonoBehaviour, IReceive<ModifiersChanged>, IReceive<DeathEvent> {

        [SerializeField] private TextMeshProUGUI _textHolder = null;
        [SerializeField] private UIHealthBarStatic[] _vitals = new UIHealthBarStatic[2];
        [SerializeField] private Image _sliderBackground = null;
        [SerializeField] private float _textSpeed = 0.075f;
        //[SerializeField] private float _actorDistance = 35f;
        [SerializeField] private float _clearActorTime = 2f;
        [SerializeField] private float _clearTextTime = 0.5f;
        [SerializeField] private GameObject _modPrefab = null;
        [SerializeField] private Transform _modGrid = null;

        private static UICenterTarget _main;
        private static Task _currentWriting;
        private static CharacterTemplate _character;
        private static bool _actorLock = false;
        private static bool _lockedText = false;
        private static string _queuedText;
        private List<UIModIcon> _active = new List<UIModIcon>();
        private List<ModEntry> _mods = new List<ModEntry>();
        //private RaycastHit[] _hits = new RaycastHit[10];
        //private Ray _mouseRay;
        private static TriggerableUnscaledTimer _clearTextTimer = new TriggerableUnscaledTimer();

        public static CharacterTemplate LockedActor {
            get {
                if (_main == null) {
                    return null;
                }

                return _actorLock ? _character : null;
            }
        }

        public static CharacterTemplate CurrentCharacter { get { return _character; } }

        void Awake() {
            _main = this;
        }

        void Update() {
            if (!Game.GameActive || _actorLock) {
                return;
            }
            if (_character != null && (_character.Disposed || _character.Entity.Tags.Contain(EntityTags.IsDead))) {
                RemoveActor();
                return;
            }
            //_mouseRay = Player.Camera.ScreenPointToRay(Input.mousePosition);
            //var count = Physics.RaycastNonAlloc(_mouseRay, _hits, _actorDistance, LayerMasks.ActorCollision);
            //_hits.SortByDistanceAsc(count);
            //for (int i = 0; i < count; i++) {
            //    if (_hits[i].transform == Player.Tr) {
            //        continue;
            //    }
            //    SceneEntity actor;
            //    if (SceneEntity.Actors.TryGetValue(_hits[i].collider, out actor)) {
            //        SetTargetActor(actor);
            //        foundActor = true;
            //        break;
            //    }
            //}
            if (_clearTextTimer.Triggered && !_clearTextTimer.IsActive) {
                _clearTextTimer.Triggered = false;
                ClearCurrentText();
            }
        }

        public static void SetText(string text, bool locked = false) {
            if (_actorLock) {
                return;
            }
            _main.SetTextOnly(text, locked);
        }

        public static void Clear(bool clearLock = false) {
            if (clearLock) {
                _lockedText = false;
                _queuedText = "";
            }
            if (_actorLock || _clearTextTimer.Triggered) {
                return;
            }
            _clearTextTimer.StartNewTime(_character != null ? _main._clearActorTime : _main._clearTextTime);
        }

        private static void ClearCurrentText() {
            if (_actorLock) {
                return;
            }
            if (_character != null) {
                _main.RemoveActor();
                return;
            }
            if (_lockedText && _queuedText == _main._textHolder.text) {
                return;
            }
            _main._textHolder.text = "";
            if (_lockedText) {
                _main.SetTextOnly(_queuedText, _lockedText);
            }
        }

        public static void ToggleActorLock() {
            if (_character == null) {
                _actorLock = false;
            }
            else {
                _actorLock = !_actorLock;
            }
            _main._sliderBackground.enabled = _actorLock;
        }

        /// <summary>
        /// Should showing target hit points be locked behind a skill?
        /// Maybe also add a right click option to show more stats behind skill/spell
        /// </summary>
        /// <param name="actor"></param>
        public static void SetTargetActor(CharacterTemplate actor) {
            if (_actorLock || _character == actor) {
                return;
            }
            _clearTextTimer.Triggered = false;
            if (_character != null) {
                _main.RemoveActor();
            }
            _character = actor;
            if (_character == null) {
                return;
            }
            _main._textHolder.maxVisibleCharacters = 0;
            _main._textHolder.text = _character.Label.Text;
            if (_currentWriting != null) {
                TimeManager.Cancel(_currentWriting);
            }
            _currentWriting = TimeManager.StartUnscaled(_main.RevealText(), _main.CurrentNull);
            if (_character != null) {
                _character.Entity.AddObserver(_main);
            }
            for (int i = 0; i < _main._vitals.Length; i++) {
                _main._vitals[i].SetNewTarget(_character);
            }
            _main.CheckMods();
            _character.Entity.AddObserver(_main);
        }

        private void SetTextOnly(string text, bool lockTxt) {
            if (_character != null && !_lockedText) {
                _queuedText = text;
                return;
            }
            if (lockTxt) {
                _lockedText = true;
                _queuedText = text;
                if (_character != null) {
                    return;
                }
            }
            _clearTextTimer.Triggered = false;
            if (_textHolder.text == text) {
                if (_currentWriting == null && _textHolder.maxVisibleCharacters < text.Length) {
                    _textHolder.maxVisibleCharacters = text.Length + 1;
                }
                return;
            }
            _textHolder.maxVisibleCharacters = 0;
            _textHolder.text = text;
            if (_currentWriting != null) {
                TimeManager.Cancel(_currentWriting);
            }
            _currentWriting = TimeManager.StartUnscaled(RevealText(), CurrentNull);
        }

        private void CurrentNull() {
            _currentWriting = null;
        }

        private IEnumerator RevealText() {
            for (int i = 0; i < _textHolder.text.Length; i++) {
                _textHolder.maxVisibleCharacters++;
                yield return _textSpeed;
            }
        }

        private void RemoveActor() {
            if (_character != null) {
                _character.Entity.RemoveObserver(this);
            }
            ClearModList();
            for (int i = 0; i < _main._vitals.Length; i++) {
                _main._vitals[i].RemoveActor();
            }
            _sliderBackground.enabled = false;
            _character = null;
            _actorLock = false;
            _textHolder.text = "";
            if (!string.IsNullOrEmpty(_queuedText)) {
                SetTextOnly(_queuedText, _lockedText);
                if (!_lockedText) {
                    _queuedText = "";
                }
            }
        }

        private void ClearModList() {
            for (int i = 0; i < _active.Count; i++) {
                ItemPool.Despawn(_active[i].gameObject);
            }
            _active.Clear();
        }

        public void Handle(ModifiersChanged arg) {
            CheckMods();
        }

        private void CheckMods() {
            if (_character == null) {
                return;
            }
            ClearModList();
            _mods.Clear();
            World.Get<ModifierSystem>().FillModList(_mods, _character.Entity.Id);
            for (int i = 0; i < _mods.Count; i++) {
                var mod = _mods[i];
                if (mod.Icon == null) {
                    continue;
                }
                var modWatch = ItemPool.SpawnUIPrefab<UIModIcon>(_modPrefab.gameObject, _modGrid);
                _active.Add(modWatch);
                modWatch.Assign(mod);
            }
        }

        public void Handle(DeathEvent arg) {
            if (_character != null && _character.Entity == arg.Target) {
                RemoveActor();
            }
        }
    }
}