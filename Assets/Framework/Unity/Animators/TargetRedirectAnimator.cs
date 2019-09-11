using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class TargetRedirectAnimator : TargetAnimator {

        [SerializeField] private TargetAnimator[] _targetAnimators = new TargetAnimator[1];

        public override float Length {
            get {
                float time = -1;
                for (int i = 0; i < _targetAnimators.Length; i++) {
                    if (_targetAnimators[i].Length > time) {
                        time = _targetAnimators[i].Length;
                    }
                }
                return time;
            }
        }

        public override bool IsPlaying {
            get {
                for (int i = 0; i < _targetAnimators.Length; i++) {
                    if (_targetAnimators[i].IsPlaying) {
                        return true;
                    }
                }
                return false;
            }
        }

        [Button("Play")]
        public override void Play() {
            for (int i = 0; i < _targetAnimators.Length; i++) {
                _targetAnimators[i].Play();
            }
        }

        public override void PlayFrame(float normalized) {
            for (int i = 0; i < _targetAnimators.Length; i++) {
                _targetAnimators[i].PlayFrame(normalized);
            }
        }
    }
}
