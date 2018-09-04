using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class Camera2D : MonoSingleton<Camera2D> {

        [SerializeField] private Camera _cam = null;
        [SerializeField] private Skybox _skybox = null;

        private static Vector3 _pos;
        private static Quaternion _rot;

        public static Camera Cam { get { return main._cam; } }
        public static Skybox Skybox { get { return main._skybox; } }
        public static Transform Tr { get; private set; }

        public static void TakeCameraControl(bool status) {
            main._cam.enabled = status;
            if (status) {
                _pos = main._cam.transform.position;
                _rot = main._cam.transform.rotation;
            }
            else {
                main._cam.transform.position = _pos;
                main._cam.transform.rotation = _rot;
            }
            main._cam.orthographic = !status;
        }

        void Awake() {
            Tr = transform;
            //_defaultPos = Tr.localPosition;
            //_defaultRot = Tr.localRotation;
        }
    }
}
