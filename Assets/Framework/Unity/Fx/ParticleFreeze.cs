using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class ParticleFreeze : MonoBehaviour, IPoolEvents, IOnCreate {
        private ParticleSystem[] _particles;
        private bool _frozen = false;

        public void OnCreate(PrefabEntity entity) {
            _particles = GetComponentsInChildren<ParticleSystem>();
        }

        public void OnPoolSpawned() {
            MessageKit.addObserver(Messages.PauseChanged, CheckPause);
        }

        public void OnPoolDespawned() {
            MessageKit.removeObserver(Messages.PauseChanged, CheckPause);
        }

        private void CheckPause() {
            if (_frozen == Game.Paused) {
                return;
            }
            _frozen = Game.Paused;
            for (int i = 0; i < _particles.Length; i++) {
                if (_frozen) {
                    _particles[i].Pause(true);
                }
                else {
                    _particles[i].Play(true);
                }
            }
        }
    }
}