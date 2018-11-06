using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PlayerCamera : MonoSingleton<PlayerCamera> {
        private static Camera _cam;

        private static Camera _altCam;
        public static Camera AltCam {
            get {
                return _altCam;
            }
            set {
                _altCam = value;
                Game.SpriteCamera = _altCam != null ? _altCam : _cam;
                WorldControlMonitor.SetCamera(_altCam != null ? _altCam : _cam);
            }
        }

        public static Camera Cam {
            get {
                if (AltCam != null) {
                    return AltCam;
                }
                if (_cam == null) {
                    _cam = main.gameObject.GetComponent<Camera>();
                }
                return _cam;
            }
        }
        public static AudioListener Listener;

        protected virtual void Awake() {
            _cam = GetComponent<Camera>();
            Game.SpriteCamera = _cam;
            Listener = GetComponent<AudioListener>();
            WorldControlMonitor.SetCamera(Player.Cam);
        }
    }
}
