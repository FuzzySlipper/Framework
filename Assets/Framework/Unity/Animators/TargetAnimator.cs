using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class TargetAnimator : MonoBehaviour, IPoolEvents {

        [SerializeField] private string _description = "";
        [SerializeField] private bool _autoStart = false;

        public string Description { get { return _description; } }
        public override string ToString() { return string.IsNullOrEmpty(_description) ? base.ToString() : _description; }

        public abstract void Play();
        public abstract void PlayFrame(float normalized);
        public abstract float Length { get; }
        public abstract bool IsPlaying { get; }

        void Awake() {
            if (_autoStart) {
                Play();
            }
        }

        public virtual void OnPoolSpawned() { }
        public virtual void OnPoolDespawned() { }
    }
}
