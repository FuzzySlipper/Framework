using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace PixelComrades {
    public class UIAnimation : MonoBehaviour, IPoolEvents {

        [SerializeField] private SimpleAnimation _animation = null;
        [SerializeField] private Image _image = null;
        [SerializeField] private bool _playOnSpawn = true;
        [SerializeField] private bool _despawnOnComplete = true;
        [SerializeField] private bool _unscaled = true;

        private int _currentFrameIndex = 0;
        private RectTransform _rect;

        public void OnPoolSpawned() {
            if (transform.parent != null) {
                SetupRect();
            }
            if (_playOnSpawn && _animation != null && _image != null) {
                Play();
            }
        }

        public void OnPoolDespawned() {}

        public void Play(SimpleAnimation anim, Material mat) {
            _animation = anim;
            if (_animation == null || _image == null) {
                ItemPool.Despawn(gameObject);
                return;
            }
            if (mat != null) {
                _image.material = mat;
            }
            if (transform.parent != null) {
                SetupRect();
            }
            Play();
        }

        private void Play() {
            TimeManager.StartTask(PlayAnimation(), _unscaled);
        }

        private void SetupRect() {
            if (_rect == null) {
                _rect = transform as RectTransform;
            }
            if (_rect == null) {
                Debug.LogErrorFormat("{0} {1} source {2} image tried to spawn as RecTransform but is not one", name, _animation != null ? _animation.name : "null", _image != null ? _image.name : "null");
                ItemPool.Despawn(gameObject);
                return;
            }
            _rect.anchorMin = Vector2.zero;
            _rect.anchorMax = Vector2.one;
            _rect.anchoredPosition = Vector2.zero;
            _rect.localScale = Vector3.one;
            _rect.sizeDelta = Vector2.zero;
        }

        private IEnumerator PlayAnimation() {
            _currentFrameIndex = 0;
            while (true) {
                _image.sprite = _animation.GetSpriteFrame(_currentFrameIndex);
                yield return _animation.GetFrameClamped(_currentFrameIndex).Length * _animation.FrameTime;
                _currentFrameIndex++;
                if (_currentFrameIndex >= _animation.LengthFrames) {
                    break;
                }
            }
            if (_despawnOnComplete) {
                ItemPool.Despawn(gameObject);
            }
        }

    }
}
