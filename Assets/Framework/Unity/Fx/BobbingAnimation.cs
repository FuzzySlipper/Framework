using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class BobbingAnimation : MonoBehaviour {

        [SerializeField] private Vector3 _bobVelocity = new Vector3(1, 0, 1);

        private Vector3 _original;
        private float _offset;

        void Awake() {
            _original = transform.position;
            _offset = Random.Range(1, 30);
        }

        void Update() {
            transform.position = _original + (Mathf.Sin(Time.time + _offset) * _bobVelocity);
        }
    }
}
