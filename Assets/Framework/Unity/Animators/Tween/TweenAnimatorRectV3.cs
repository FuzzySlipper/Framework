using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class TweenAnimatorRectV3 : TweenAnimator {

        [SerializeField] private Vector3[] _targets = new Vector3[0];
        [SerializeField] private EasingTypes[] _easing = new EasingTypes[0];
        [SerializeField] private float[] _durations = new float[0];

        private int _index = -1;
        private TweenV3 _tweener = new TweenV3();
        private RectTransform _rectTr = null;

        public override Tweener Tween { get { return _tweener; } }

        public override void StartTween() {
            _rectTr = (RectTransform) Target;
            _index++;
            if (_index >= _targets.Length) {
                _index = 0;
            }
            var origin = _rectTr.anchoredPosition3D;
            _tweener.Restart(origin, _targets[_index], _durations[_index], _easing[_index], UnScaled);
        }

        public override void UpdateTween() {
            _rectTr.anchoredPosition3D = _tweener.Get();
        }
    }
}
