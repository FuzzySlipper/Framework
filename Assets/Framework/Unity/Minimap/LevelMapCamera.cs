using UnityEngine;
using System.Collections;
namespace PixelComrades {
    public class LevelMapCamera : MonoSingleton<LevelMapCamera> {

        [SerializeField] private Camera _camera = null;
        
        public Camera MapCamera { get { return _camera; } }

        void Start() {
            _camera.enabled = false;
        }
        
        public void Toggle() {
            SetStatus(!_camera.enabled);
        }

        public void SetStatus(bool status) {
            _camera.enabled = status;
            if (_camera.enabled) {
                transform.position = Player.Tr.position + new Vector3(0, 10, 0);
            }
        }
    }
}