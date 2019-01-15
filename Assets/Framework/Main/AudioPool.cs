using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class AudioPool {

        private static Dictionary<string, AudioClip> _clipDictionary = new Dictionary<string, AudioClip>();
        private static UnscaledTimer _bufferTimer = new UnscaledTimer(0.35f);

        private static int _maxSources = 32;

        private static Queue<AudioSource> _sourcePool = new Queue<AudioSource>();
        private static List<AudioSource> _currentSources = new List<AudioSource>();

        public static void PlayClipBuffered(AudioClip clip, Vector3 position, float spatialBlend = 0.5f, float volume = 1) {
            if (_bufferTimer.IsActive) {
                return;
            }
            _bufferTimer.StartTimer();
            PlayClip(clip, position, spatialBlend);
        }

        public static void PlayClip(AudioClip clip, Vector3 position, float spatialBlend = 0.5f, float volume = 1) {
            CheckCurrent();
            Play(clip,position, spatialBlend);
        }

        public static void PlayClip2D(AudioClip clip, float volume = 1) {
            CheckCurrent();
            Play(clip, volume);
        }

        public static void PlayClip(string clipName, Vector3 position, float spatialBlend = 0.5f, float volume = 1) {
            AudioClip clip;
            if (!_clipDictionary.TryGetValue(clipName, out clip)) {
                clip = (AudioClip)Resources.Load(clipName);
                if (clip == null) {
                    Debug.LogErrorFormat("Couldn't find clip {0}", clipName);
                    return;
                }
                _clipDictionary.Add(clipName, clip);
            }
            CheckCurrent();
            Play(clip, position);
        }

        private static void Play(AudioClip clip, Vector3 position, float spatialBlend = 0, float volume = 1) {
            AudioSource clipSource = null;
            if (_sourcePool.Count > 0) {
                clipSource = _sourcePool.Dequeue();
            }
            else {
                if (_currentSources.Count < _maxSources) {
                    clipSource = AddSource();
                }
            }
            if (clipSource == null) {
                return;
            }
            clipSource.clip = clip;
            clipSource.transform.position = position;
            clipSource.spatialBlend = spatialBlend;
            clipSource.volume = volume;
            clipSource.Play();
            _currentSources.Add(clipSource);
        }

        private static void Play(AudioClip clip, float volume = 1) {
            AudioSource clipSource = null;
            if (_sourcePool.Count > 0) {
                clipSource = _sourcePool.Dequeue();
            }
            else {
                if (_currentSources.Count < _maxSources) {
                    clipSource = AddSource();
                }
            }
            if (clipSource == null) {
                return;
            }
            clipSource.clip = clip;
            clipSource.spatialBlend = 0;
            clipSource.volume = volume;
            clipSource.Play();
            _currentSources.Add(clipSource);
        }

        private static AudioSource AddSource() {
            var newSource = new GameObject("AudioPool");
            newSource.transform.SetParent(Pivot);
            return newSource.AddComponent<AudioSource>();
        }

        private static void CheckCurrent() {
            for (int i = _currentSources.Count - 1; i >= 0; i--) {
                if (!_currentSources[i].isPlaying) {
                    _sourcePool.Enqueue(_currentSources[i]);
                    _currentSources.RemoveAt(i);
                }
            }
        }

        private static Transform _pivot = null;
        private static Transform Pivot {
            get {
                if (_pivot == null) {
                    _pivot = Game.GetMainChild("AudioPool");
                }
                return _pivot;
            }
        }
    }
}
