using UnityEngine;
using UnityEngine.AI;

namespace PixelComrades {
    public class NavMeshAgentFreeze : MonoBehaviour {

        private NavMeshAgent _agent;
        private bool _frozen = false;
        private bool _wasFrozen = false;

        void Awake() {
            _agent = GetComponent<NavMeshAgent>();
        }

        void OnEnable() {
            MessageKit.addObserver(Messages.PauseChanged, CheckPause);
        }

        void OnDisable() {
            MessageKit.removeObserver(Messages.PauseChanged, CheckPause);
        }

        private void CheckPause() {
            if (_frozen == Game.Paused || !_agent.isOnNavMesh) {
                return;
            }
            if (!_frozen && !_agent.isStopped && Game.Paused) {
                _wasFrozen = true;
            }
            _frozen = Game.Paused;
            if (_frozen && !_agent.isStopped) {
                _agent.isStopped = true;
            }
            else if (!_frozen && _wasFrozen) {
                _agent.isStopped = false;
                _wasFrozen = false;
            }
        }
    }
}