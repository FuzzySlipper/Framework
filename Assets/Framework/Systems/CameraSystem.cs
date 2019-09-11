using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace PixelComrades {
    //public class CameraSystem : SystemBase, IReceiveGlobalArray<CameraShakeEvent> {

    //    public static Action<float> ShakeEvent;

    //    private bool _isShaking;

    //    public void HandleGlobal(ManagedArray<CameraShakeEvent> arg) {
    //        if (!GameOptions.UseShaking || _isShaking) {
    //            return;
    //        }
    //        float shake = 0;
    //        for (int i = 0; i < arg.Count; i++) {
    //            shake += arg[i].Intensity;
    //        }
    //        if (shake > 0.5f) {
    //            ShakeOneShot(shake);
    //        }
    //    }

    //    public void ShakeOneShot(float intensity) {
    //        TimeManager.StartUnscaled(StartShake(.2f, intensity));
    //    }

    //    private IEnumerator StartShake(float duration, float intensity) {
    //        _isShaking = true;
    //        //var camTr = Player.Cam.transform;
    //        //Vector3 originalPos = camTr.localPosition;
    //        //Quaternion originalRot = camTr.localRotation;
    //        //originalRot.eulerAngles = Vector3.up * camTr.localEulerAngles.y;
    //        var start = TimeManager.TimeUnscaled;
    //        var percent = 0f;
    //        while (percent < 1) {
    //            percent = (TimeManager.TimeUnscaled - start) / duration * 0.75f;

    //            var damper = 1f - Mathf.Clamp01(4f * percent - 3f);
    //            var shakeRange = damper * Random.Range(-intensity, intensity);

    //            var shakePos = Random.insideUnitCircle * shakeRange * .035f;
    //            var shakeRot = new Vector2(-shakeRange * .15f, shakeRange * 1.75f);
    //            FirstPersonCamera.AddPosition += shakePos;
    //            FirstPersonCamera.AddRotation += shakeRot;
    //            //camTr.localPosition = Vector3.Lerp(camTr.localPosition, shakePos, Time.smoothDeltaTime * 2f);
    //            //camTr.localRotation = Quaternion.Slerp(camTr.localRotation, shakeRot, Time.smoothDeltaTime * 2f);

    //            yield return null;
    //        }
    //        //var endPos = camTr.localPosition;
    //        //var endRot = camTr.localRotation;
    //        //percent = 0;
    //        //while (percent < 1) {
    //        //    percent = (TimeManager.TimeUnscaled - start) / duration * 0.25f;
    //        //    camTr.localPosition = Vector3.Lerp(endPos, originalPos, percent);
    //        //    camTr.localRotation = Quaternion.Slerp(endRot, originalRot, percent);
    //        //    yield return null;
    //        //}
    //        _isShaking = false;
    //    }
    //}

}
