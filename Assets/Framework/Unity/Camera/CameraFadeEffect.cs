using UnityEngine;

namespace PixelComrades {
    [ExecuteInEditMode]
    public class CameraFadeEffect : MonoBehaviour {

        public Shader CameraFadeShader;
        [Range(0, 1)] public float FadeFactor;
        public RenderTexture FadeFrom;
        public Camera Cam;

        private Material _material;

        private void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (FadeFrom == null) {
                return;
            }
            if (_material == null) {
                if (CameraFadeShader == null) {
                    return;
                }
                _material = new Material(CameraFadeShader);
                _material.hideFlags = HideFlags.HideAndDontSave;
            }
            _material.SetFloat("_FadeFactor", FadeFactor);

            //Blit additively onto the destination using different passes for (1 - fade) and (fade) multipliers.
            Graphics.Blit(source, destination, _material, 0);
            Graphics.Blit(FadeFrom, destination, _material, 1);
        }
    }
}