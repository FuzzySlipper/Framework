using UnityEngine;

namespace PixelComrades {

    public interface IRenderWatcherReceiver {
        void SetIsRendered(bool status);
    }

    public class RendererWatcher : MonoBehaviour, IOnCreate {

        [SerializeField] private GameObject[] _receiverTargets = new GameObject[0];

        private IRenderWatcherReceiver[] _receivers;
        
        public void OnCreate(PrefabEntity entity) {
            _receivers = new IRenderWatcherReceiver[_receiverTargets.Length];
            for (int i = 0; i < _receiverTargets.Length; i++) {
                _receivers[i] = _receiverTargets[i].GetComponent<IRenderWatcherReceiver>();
            }
        }

        void OnBecameVisible() {
            if (_receivers == null) {
                return;
            }
            for (int i = 0; i < _receivers.Length; i++) {
                if (_receivers[i] != null) {
                    _receivers[i].SetIsRendered(true);
                }
            }
        }

        void OnBecameInvisible() {
            if (_receivers == null) {
                return;
            }
            for (int i = 0; i < _receivers.Length; i++) {
                if (_receivers[i] != null) {
                    _receivers[i].SetIsRendered(false);
                }
            }
        }
    }
}