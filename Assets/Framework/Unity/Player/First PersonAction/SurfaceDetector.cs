using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SurfaceDetector : ScriptableObject {

    private static SurfaceDetector instance;

    public BaseSurface genericSurface = null;
    public SurfaceData[] surfaces = null;

    public static SurfaceDetector main {
        get {
            if (instance == null) {
                instance = Resources.Load<SurfaceDetector>("SurfaceDetector");
            }
            return instance;
        }
    }

    public string[] GetNames {
        get {
            var size = surfaces.Length;
            var tmpNames = new string[size];

            for (var i = 0; i < size; i++) {
                tmpNames[i] = surfaces[i].name;
            }

            return tmpNames;
        }
    }

    [Serializable]
    public class BaseSurface {
        public AudioClip[] footstepSounds = null;
        public AudioClip jumpingSound, landigSound;
    }

    [Serializable]
    public sealed class SurfaceData : BaseSurface {
        public Material[] materials = null;
        public string name = string.Empty;
    }

    internal void PlayFootStepSound(FirstPersonController controller) {
        controller.Audio.pitch = Time.timeScale;
        var surface = controller.CurrentSurface;

        if (surface != null) {
            PlayRandomFootstep(surface.footstepSounds, controller);
        }
        else {
            PlayRandomFootstep(genericSurface.footstepSounds, controller);
        }
    }

    public void PlayJumpingSound(FirstPersonController controller) {
        controller.Audio.pitch = Time.timeScale;
        var surface = controller.CurrentSurface;
        if (surface != null) {
            controller.Audio.PlayOneShot(surface.jumpingSound, controller.Settings.SoundsVolume);
        }
        else {
            controller.Audio.PlayOneShot(genericSurface.jumpingSound, controller.Settings.SoundsVolume);
        }
    }

    internal void PlayLandingSound(FirstPersonController controller) {
        controller.Audio.pitch = Time.timeScale;
        var surface = controller.CurrentSurface;
        if (surface != null) {
            controller.Audio.PlayOneShot(surface.landigSound, controller.Settings.SoundsVolume);
        }
        else {
            controller.Audio.PlayOneShot(genericSurface.landigSound, controller.Settings.SoundsVolume);
        }
    }

    private void PlayRandomFootstep(AudioClip[] stepSounds, FirstPersonController controller) {
        if (stepSounds == null) {
            return;
        }
        var index = Random.Range(1, stepSounds.Length);
        controller.Audio.clip = stepSounds[index];
        controller.Audio.PlayOneShot(controller.Audio.clip, controller.Settings.SoundsVolume);
        stepSounds[index] = stepSounds[0];
        stepSounds[0] = controller.Audio.clip;
    }

    public static Material GetMaterial(RaycastHit hit) {
        if (hit.collider== null) {
            return null;
        }
        var rend = hit.collider.GetComponent<Renderer>();
        if (!rend) {
            return null;
        }
        var meshCollider = hit.collider.GetComponent<MeshCollider>();
        if (meshCollider && !meshCollider.convex) {
            var mesh = meshCollider.sharedMesh;
            var tIndex = hit.triangleIndex * 3;
            var index1 = mesh.triangles[tIndex];
            var index2 = mesh.triangles[tIndex + 1];
            var index3 = mesh.triangles[tIndex + 2];
            var subMeshCount = mesh.subMeshCount;

            int[] triangles = null;

            for (var i = 0; i < subMeshCount; i++) {
                triangles = mesh.GetTriangles(i);

                for (var j = 0; j < triangles.Length; j += 3) {
                    if (triangles[j] == index1 && triangles[j + 1] == index2 && triangles[j + 2] == index3) {
                        return rend.sharedMaterials[i];
                    }
                }
            }
        }
        else {
            return rend.sharedMaterial;
        }
        return null;
    }

    public SurfaceData GetSurface(RaycastHit hit) {
        var tmpMat = GetMaterial(hit);
        if (tmpMat == null) {
            return null;
        }
        for (int i = 0; i < surfaces.Length; i++) {
            for (int mat = 0; mat < surfaces[i].materials.Length; mat++) {
                if (surfaces[i].materials[mat] == tmpMat) {
                    return surfaces[i];
                }
            }
        }
        return null;
    }
}