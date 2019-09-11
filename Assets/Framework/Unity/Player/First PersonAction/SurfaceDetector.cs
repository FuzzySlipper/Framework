using System;
using PixelComrades;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PixelComrades {
    public class SurfaceDetector : ScriptableObject {

        private static SurfaceDetector _instance;

        public BaseSurface GenericSurface = null;
        public SurfaceData[] Surfaces = null;

        public static SurfaceDetector main {
            get {
                if (_instance == null) {
                    _instance = Resources.Load<SurfaceDetector>("SurfaceDetector");
                }
                return _instance;
            }
        }

        public string[] GetNames {
            get {
                var size = Surfaces.Length;
                var tmpNames = new string[size];

                for (var i = 0; i < size; i++) {
                    tmpNames[i] = Surfaces[i].SurfaceName;
                }

                return tmpNames;
            }
        }

        [Serializable]
        public class BaseSurface {
            public AudioClip[] FootstepSounds = null;
            public AudioClip JumpingSound, LandingSound;
        }

        [Serializable]
        public sealed class SurfaceData : BaseSurface {
            public Material[] Materials = null;
            public string SurfaceName = string.Empty;
        }

        internal void PlayFootStepSound(IFirstPersonController controller) {
            controller.Audio.pitch = Time.timeScale;
            var surface = controller.CurrentSurface;

            if (surface != null) {
                PlayRandomFootstep(surface.FootstepSounds, controller);
            }
            else {
                PlayRandomFootstep(GenericSurface.FootstepSounds, controller);
            }
        }

        public void PlayJumpingSound(IFirstPersonController controller) {
            controller.Audio.pitch = Time.timeScale;
            var surface = controller.CurrentSurface;
            if (surface != null) {
                controller.Audio.PlayOneShot(surface.JumpingSound, controller.SoundVolume);
            }
            else {
                controller.Audio.PlayOneShot(GenericSurface.JumpingSound, controller.SoundVolume);
            }
        }

        internal void PlayLandingSound(IFirstPersonController controller) {
            controller.Audio.pitch = Time.timeScale;
            var surface = controller.CurrentSurface;
            if (surface != null) {
                controller.Audio.PlayOneShot(surface.JumpingSound, controller.SoundVolume);
            }
            else {
                controller.Audio.PlayOneShot(GenericSurface.LandingSound, controller.SoundVolume);
            }
        }

        private void PlayRandomFootstep(AudioClip[] stepSounds, IFirstPersonController controller) {
            if (stepSounds == null) {
                return;
            }
            var index = Random.Range(1, stepSounds.Length);
            controller.Audio.clip = stepSounds[index];
            controller.Audio.PlayOneShot(controller.Audio.clip, controller.SoundVolume);
            stepSounds[index] = stepSounds[0];
            stepSounds[0] = controller.Audio.clip;
        }

        public static Material GetMaterial(RaycastHit hit) {
            if (hit.collider == null) {
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
            for (int i = 0; i < Surfaces.Length; i++) {
                for (int mat = 0; mat < Surfaces[i].Materials.Length; mat++) {
                    if (Surfaces[i].Materials[mat] == tmpMat) {
                        return Surfaces[i];
                    }
                }
            }
            return null;
        }
    }
}