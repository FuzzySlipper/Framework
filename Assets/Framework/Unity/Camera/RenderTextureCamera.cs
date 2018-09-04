using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace PixelComrades {
    public class RenderTextureCamera : MonoSingleton<RenderTextureCamera> {

        [SerializeField] private Camera _camera;

        private static FieldInfo _canvasHackField;
        private static object _canvasHackObject;
        private static Camera Camera { get { return main._camera; } }

        void Awake() {
            _canvasHackField = typeof(Canvas).GetField("willRenderCanvases", BindingFlags.NonPublic | BindingFlags.Static);
            _canvasHackObject = _canvasHackField.GetValue(null);
        }

        public static void TakePicture(Transform parent, DirectionsEight dir, float offset, float height, float size, RenderTexture texture) {
            Camera.enabled = true;
            Camera.transform.parent = parent;
            Camera.targetTexture = texture;
            Camera.orthographicSize = size;
            SpriteFacingControl.SetCameraPos(Camera, dir, offset, height);
            _canvasHackField.SetValue(null, null);
            Camera.Render();
            _canvasHackField.SetValue(null, _canvasHackObject);
            Camera.Render();
            Camera.enabled = false;
            //Camera.targetTexture = null;
            Camera.transform.parent = null;
        }
    }
}
