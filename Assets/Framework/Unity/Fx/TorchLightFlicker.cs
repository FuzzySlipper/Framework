using System.Collections;
using UnityEngine;

namespace PixelComrades {
    public class TorchLightFlicker : MonoBehaviour, ISystemUpdate, IOnCreate {

        [SerializeField] private float _freq = 4f;
        [SerializeField] private float _amp = 0.05f;
        [SerializeField] private bool _unscaled = false;
        [SerializeField] private bool _active = true;

        private Light _light;
        private Color _lightColor;
        public bool Unscaled { get { return _unscaled; } }

        public void OnCreate(PrefabEntity entity) {
            _light = GetComponent<Light>();
            _lightColor = _light.color;
        }

        public void OnSystemUpdate(float dt) {
            if (!_active) {
                return;
            }
            if (_light == null) {
                OnCreate(null);
            }
            _light.color = _lightColor * GetLightAnimation();
        }

        [UnityEngine.ContextMenu("Test")] private void TestLight() {
            _light = GetComponent<Light>();
            _lightColor = _light.color;
            TimeManager.StartUnscaled(LightTester());
        }

        private IEnumerator LightTester() {
            var endTime = TimeManager.Time + 5;
            while (TimeManager.Time < endTime) {
                _light.color = _lightColor * GetLightAnimation();
                yield return null;
            }
            _light.color = _lightColor;
        }

        private float GetLightAnimation() {
            return -Mathf.Sin((TimeManager.Time - Mathf.Floor(TimeManager.Time)) * _freq * 3.14159274f) * _amp + 0.95f;
        }
    }
}