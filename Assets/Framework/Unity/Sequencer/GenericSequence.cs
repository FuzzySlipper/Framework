using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [CreateAssetMenu]
    public class GenericSequence : ScriptableObject {
        
        [SerializeField] private List<SequenceObject> _objects = new List<SequenceObject>();
        [SerializeField] private int _maxTracks = 2;
        [SerializeField] private float _maxDuration = 1f;

        public bool Looping;
        
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
        public List<SequenceObject> Objects { get { return _objects; } }


        public void Add(SequenceObject animObject) {
            _objects.Add(animObject);
        }

        public void Remove(SequenceObject animObject) {
            _objects.Remove(animObject);
        }

        public float FindEndTime() {
            var max = 0f;
            for (int i = 0; i < _objects.Count; i++) {
                max = Mathf.Max(max, _objects[i].EndTime);
            }
            return max;
        }
    }
}
