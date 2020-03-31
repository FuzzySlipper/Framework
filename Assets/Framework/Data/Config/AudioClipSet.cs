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
            public AudioClipAssetReference Audio;
            [HideInInspector] public AudioClip LoadedAudio;

            private ValueDropdownList<int> SignalsList() {
                return EntitySignals.GetDropdownList();
            }
        }
        public List<Clip> Clips = new List<Clip>();

        private Dictionary<int, AudioClip> _dict = new Dictionary<int, AudioClip>();

        public void Load() {
            for (int i = 0; i < Clips.Count; i++) {
                var clip = Clips[i];
                clip.Audio.LoadAssetAsync().Completed += handle => clip.LoadedAudio = handle.Result;
            }
        }

        public void Unload() {
            for (int i = 0; i < Clips.Count; i++) {
                var clip = Clips[i];
                clip.Audio.ReleaseAsset();
                clip.LoadedAudio = null;
            }
        }
        
        public void PlayAudio(int signal, Vector3 position) {
            if (_dict.Count == 0) {
                for (int i = 0; i < Clips.Count; i++) {
                    if (!_dict.ContainsKey(Clips[i].Id)) {
                        _dict.Add(Clips[i].Id, Clips[i].LoadedAudio);
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
