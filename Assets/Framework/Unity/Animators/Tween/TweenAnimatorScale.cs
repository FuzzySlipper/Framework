using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class TweenAnimatorScale : TweenAnimator {

        [SerializeField] private TweenV3 _tweener = new TweenV3();
        [SerializeField] private Vector3[] _targets = new Vector3[0];
        [SerializeField] private EasingTypes[] _easing = new EasingTypes[0];
        [SerializeField] private float[] _durations = new float[0];
        private int _index = -1;

        public override Tweener Tween { get { return _tweener; } }
        public override void StartTween() {
            if (IsInvalid) {
                return;
            }
            _index++;
            if (_index >= _targets.Length) {
                _index = 0;
            }
            var source = Target.localScale;
            if (_easing.Length > _index) {
                _tweener.EasingConfig = _easing[_index];
            }
            _tweener.UnScaled = UnScaled;
            _tweener.Restart(source, _targets[_index], _durations.Length > _index ? _durations[_index] : _tweener.Length);
        }

        public override void UpdateTween() {
            Target.localScale = _tweener.Get();
        }

        public override void PlayFrame(float normalized) {
            Target.localScale = _tweener.Get(normalized);
        }
    }
}
