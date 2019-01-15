using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class TweenAnimatorV3 : TweenAnimator {

        [SerializeField] private bool _local = true;
        [SerializeField] private Vector3[] _targets = new Vector3[0];
        [SerializeField] private EasingTypes[] _easing = new EasingTypes[0];
        [SerializeField] private float[] _durations = new float[0];

        private int _index = -1;
        private TweenV3 _tweener = new TweenV3();

        public override Tweener Tween { get { return _tweener; } }

        public override void StartTween() {
            _index++;
            if (_index >= _targets.Length) {
                _index = 0;
            }
            var origin = _local ? Target.localPosition : Target.position;
            _tweener.Restart(origin, _targets[_index], _durations[_index], _easing[_index], UnScaled);
        }

        public override void UpdateTween() {
            if (_local) {
                Target.localPosition = _tweener.Get();
            }
            else {
                Target.position = _tweener.Get();
            }
        }
    }
}
