using UnityEngine;

namespace PixelComrades {
    public class RandomTargetAnimator : MonoBehaviour, ISystemUpdate, IPoolEvents, ILevelTriggerReceiver {

        [SerializeField] private bool _unscaled = true;
        [SerializeField] private TargetAnimator[] _targetAnimators = new TargetAnimator[0];
        [SerializeField] private bool _loop = true;
        [SerializeField] private bool _activeByDefault = true;
        [SerializeField] private TriggerTargetTypes _trigger = TriggerTargetTypes.None;

        private TargetAnimator _current = null;
        private bool _active = false;
        private KeyHole _keyHole = new KeyHole("");

        public bool IsActive { get { return _active; } set { _active = value; } }
        public TriggerTargetTypes TriggerType { get { return _trigger; } set { _trigger = value; } }
        public KeyHole Keyhole { get { return _keyHole; }}
        public bool Unscaled { get { return _unscaled; } }

        public void OnSystemUpdate(float dt) {
            if (!_loop) {
                return;
            }
            if (_current != null && !_current.IsPlaying) {
                _current = _targetAnimators.RandomElement();
                _current.Play();
            }
        }

        public void OnPoolSpawned() {
            if (!_activeByDefault) {
                return;
            }
            _active = true;
            _current = _targetAnimators.RandomElement();
            _current.Play();
        }

        public void OnPoolDespawned() {
            _current = null;
            _active = false;
        }

        public void LevelTrigger(ILevelTrigger origin) {
            if (_trigger == TriggerTargetTypes.None) {
                return;
            }
            if (_current == null) {
                _current = _targetAnimators.RandomElement();
                _current.Play();
            }
            else {
                _current = null;
            }
        }
    }
}