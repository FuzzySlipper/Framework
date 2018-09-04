using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class AudioClipSet : ScriptableObject {
        [System.Serializable]
        public class Clip {
            [ValueDropdown("SignalsList")]
            public int Id;
            public AudioClip Audio;

            private ValueDropdownList<int> SignalsList() {
                return EntitySignals.GetDropdownList();
            }
        }
        public List<Clip> Clips = new List<Clip>();

        private Dictionary<int, AudioClip> _dict = new Dictionary<int, AudioClip>();

        public void PlayAudio(int signal, Vector3 position) {
            if (_dict.Count == 0) {
                for (int i = 0; i < Clips.Count; i++) {
                    if (!_dict.ContainsKey(Clips[i].Id)) {
                        _dict.Add(Clips[i].Id, Clips[i].Audio);
                    }
                }
            }
            if (!_dict.TryGetValue(signal, out var clip)) {
                return;
            }
            AudioPool.PlayClip(clip, position);
        }
    }
}
