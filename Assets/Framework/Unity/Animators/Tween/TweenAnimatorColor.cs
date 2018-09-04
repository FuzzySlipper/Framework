using UnityEngine;
using UnityEngine.UI;

namespace PixelComrades {
    public class TweenAnimatorColor : TweenAnimator {

        [SerializeField] private Color[] _targets = new Color[0];
        [SerializeField] private EasingTypes[] _easing = new EasingTypes[0];
        [SerializeField] private float[] _durations = new float[0];
        [SerializeField] private Renderer _colorTarget;
        [SerializeField] private Image _uiColorTarget;

        private int _index = -1;
        private TweenColor _tweener = new TweenColor();

        public override Tweener Tween { get { return _tweener; } }

        public override void StartTween() {
            _index++;
            if (_index >= _targets.Length) {
                _index = 0;
            }
            var originColor = _uiColorTarget != null ? _uiColorTarget.color : _colorTarget.material.color;
            _tweener.Restart(originColor, _targets[_index], _durations[_index], _easing[_index], UnScaled);
        }

        public override void UpdateTween() {
            if (_uiColorTarget != null) {
                _uiColorTarget.color = _tweener.Get();
            }
            if (_colorTarget != null) {
                _colorTarget.material.color = _tweener.Get();
            }
        }
    }
}