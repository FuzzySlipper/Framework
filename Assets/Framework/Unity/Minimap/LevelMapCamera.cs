using UnityEngine;
using System.Collections;
namespace PixelComrades {

    public interface IMapCamera {
        void UpdateInput(Vector2 move, float scroll, bool rotateDown);
    }

    public class LevelMapCamera : MonoSingleton<LevelMapCamera>, IMapCamera {

        [SerializeField] private Camera _camera = null;
        [SerializeField] private RtsCamera _input = null;

        public void UpdateInput(Vector2 move, float scroll, bool rotateDown) {
            _input.UpdateInput(move, scroll, rotateDown);
        }

        void Start() {
            Game.LevelMap = this;
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