using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CameraSystem : SystemBase, IReceiveGlobal<CameraShakeEvent> {

        private bool _isShaking;

        public void HandleGlobal(ManagedArray<CameraShakeEvent> arg) {
            if (!GameOptions.UseShaking || _isShaking) {
                return;
            }
            float shake = 0;
            for (int i = 0; i < arg.Count; i++) {
                shake += arg[i].Intensity;
            }
            if (shake > 0.5f) {
                ShakeOneShot(shake);
            }
        }

        public void ShakeOneShot(float intensity) {
            TimeManager.StartUnscaled(StartShake(.2f, intensity));
        }

        private IEnumerator StartShake(float duration, float intensity) {
            _isShaking = true;
            var camTr = Player.Cam.transform;
            Vector3 originalPos = camTr.localPosition;
            Quaternion originalRot = camTr.localRotation;
            originalRot.eulerAngles = Vector3.up * camTr.localEulerAngles.y;
            var start = TimeManager.TimeUnscaled;
            var percent = 0f;
            while (percent < 1) {
                percent = (TimeManager.TimeUnscaled - start) / duration * 0.75f;

                var damper = 1f - Mathf.Clamp01(4f * percent - 3f);
                var shakeRange = damper * Random.Range(-intensity, intensity);

                var shakePos = originalPos + Random.insideUnitSphere * shakeRange * .035f;
                var shakeRot = originalRot * Quaternion.Euler(-shakeRange * .15f, 0f, shakeRange * 1.75f);
                camTr.localPosition = Vector3.Lerp(camTr.localPosition, shakePos, Time.smoothDeltaTime * 2f);
                camTr.localRotation = Quaternion.Slerp(camTr.localRotation, shakeRot, Time.smoothDeltaTime * 2f);

                yield return null;
            }
            var endPos = camTr.localPosition;
            var endRot = camTr.localRotation;
            percent = 0;
            while (percent < 1) {
                percent = (TimeManager.TimeUnscaled - start) / duration * 0.25f;
                camTr.localPosition = Vector3.Lerp(endPos, originalPos, percent);
                camTr.localRotation = Quaternion.Slerp(endRot, originalRot, percent);
                yield return null;
            }
            _isShaking = false;
        }
    }

    public struct CameraShakeEvent : IEntityMessage {
        public float Intensity;

        public CameraShakeEvent(float intensity = 150f) {
            Intensity = intensity;
        }
    }
}
