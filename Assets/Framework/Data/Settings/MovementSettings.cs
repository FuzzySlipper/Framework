using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class MovementSettings : ScriptableObject {
        [SerializeField] private float _speed = 10;
        [SerializeField] private float _rotationSpeed = 10;
        [SerializeField] private float _acceleration = 50;

        public float Speed {get { return _speed; } }
        public float Rotation {get { return _rotationSpeed; } }
        public float Acceleration {get { return _acceleration; } }
    }
}