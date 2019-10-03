using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [CreateAssetMenu]
    public class GenericAnimation : ScriptableObject {
        
        [SerializeField] private List<AnimationObject> _objects = new List<AnimationObject>();
        [SerializeField] private List<SimpleAnimationEvent> _events = new List<SimpleAnimationEvent>();
        [SerializeField] private int _maxTracks = 5;
        [SerializeField] private float _maxDuration = 1f;

        public bool Looping;
        
        public List<SimpleAnimationEvent> Events { get => _events; }
        public float MaxDuration {
            get { return _maxDuration; }
            set {
                var minMax = value;
                for (int i = 0; i < _objects.Count; i++) {
                    minMax = Mathf.Max(minMax,_objects[i].EndTime);
                }
                _maxDuration = value < minMax ? minMax : value;
            }
        }
        public int MaxTracks {
            get { return _maxTracks; }
            set {
                var minMax = value < 1 ? 1 : value;
                for (int i = 0; i < _objects.Count; i++) {
                    minMax = Mathf.Max(minMax, _objects[i].EditingTrack + 1);
                }
                _maxTracks = value < minMax ? minMax : value;
            }
        }
        public List<AnimationObject> Objects { get { return _objects; } }


        public void Add(AnimationObject animObject) {
            _objects.Add(animObject);
        }

        public void Remove(AnimationObject animObject) {
            _objects.Remove(animObject);
        }

        public float FindEndTime() {
            var max = 0f;
            for (int i = 0; i < _objects.Count; i++) {
                max = Mathf.Max(max, _objects[i].EndTime);
            }
            return max;
        }

        private const float EventPrecision = 0.1f;
        
        public SimpleAnimationEvent? FindEvent(float time) {
            for (int i = 0; i < _events.Count; i++) {
                if (Math.Abs(_events[i].Time - time) < EventPrecision) {
                    return _events[i];
                }
            }
            return null;
        }
    }
}
