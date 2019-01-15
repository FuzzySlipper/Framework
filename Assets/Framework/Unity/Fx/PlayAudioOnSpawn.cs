using UnityEngine;
using System.Collections;
namespace PixelComrades {
    public class PlayAudioOnSpawn : MonoBehaviour, IPoolEvents, IOnCreate {

        private AudioSource _source;

        public void OnCreate(PrefabEntity entity) {
            _source = GetComponent<AudioSource>();
        }

        public void OnPoolSpawned() {
            _source.Play();
        }

        public void OnPoolDespawned() {
            if (_source.isPlaying) {
                _source.Stop();
            }
        }
    }
}
