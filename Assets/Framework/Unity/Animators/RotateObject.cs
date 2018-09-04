using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class RotateObject : MonoBehaviour {

        public float SensitivityX = 15.0f;
        public float SensitivityY = 15.0f;
        public float Speed = 20;
        public float ZoomSpeed = 5;
        public FloatRange ZoomLimits = new FloatRange(0.01f, 50);

        void Update() {
            if (Input.GetMouseButton(1)) {
                var rotation = new Vector3(Input.GetAxis("Mouse Y") * SensitivityX,
                    Input.GetAxis("Mouse X") * SensitivityY, 0);
                transform.Rotate(rotation * Speed);
            }
            var scale = transform.localScale.x;
            scale += (Input.GetAxis("Mouse ScrollWheel")) * ZoomSpeed;
            if (scale == transform.localScale.x) {
                return;
            }
            scale = Mathf.Clamp(scale, ZoomLimits.Min, ZoomLimits.Max);
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}