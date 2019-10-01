using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public class PIDController {
        public float P = .8f;
        public float I = .0002f;
        public float D = .2f;
        public float Minimum = -1;
        public float Maximum = 1;

        [SerializeField] private float _integral;
        [SerializeField] private float _lastProportional;

        public PIDController() { }

        public PIDController(float p, float i, float d) {
            P = p;
            I = i;
            D = d;
        }

        public float Seek(float seekValue, float currentValue, float deltaTime) {
            float proportional = seekValue - currentValue;

            float derivative = (proportional - _lastProportional) / deltaTime;
            _integral += proportional * deltaTime;
            _lastProportional = proportional;

            //This is the actual PID formula. This gives us the value that is returned
            float value = P * proportional + I * _integral + D * derivative;
            value = Mathf.Clamp(value, Minimum, Maximum);

            return value;
        }
    }
}
