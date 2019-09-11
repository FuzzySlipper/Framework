using UnityEngine;
using UnityEngine.Rendering;

namespace PixelComrades {
    [ExecuteInEditMode]
    public class Decal : MonoBehaviour {
        public enum DecalRenderMode {
            Deferred,
            Unlit,
            Invalid
        }

        public Color Color;
        [Tooltip("Enable to draw the Albedo / Emission pass of the Decal.")]
        public bool DrawAlbedo = true;
        [Tooltip("Enable to draw the Normal / SpecGloss pass of the Decal.")]
        public bool DrawNormalAndGloss = true;
        [Tooltip("To which degree should the Decal be drawn? At 1, the Decal will be drawn with full effect. At 0, the Decal will not be drawn. Experiment with values greater than one.")]
        public float Fade = 1.0f;
        [Tooltip("Enable perfect Normal / SpecGloss blending between decals. Costly and has no effect when decals don't overlap, so use with caution.")]
        public bool HighQualityBlending = false;
        [Tooltip("Set a GameObject here to only draw this Decal on the MeshRenderer of the GO or any of its children.")]
        public GameObject LimitTo = null;
        [Tooltip("Set a Material with a Decalicious shader.")]
        public Material Material;
        public DecalRenderMode RenderMode = DecalRenderMode.Invalid;
        [Tooltip("Should this decal be drawn early (low number) or late (high number)?")]
        public int RenderOrder = 100;
        [Tooltip("Use an interpolated light probe for this decal for indirect light. This breaks instancing for the decal and thus comes with a performance impact, so use with caution.")]
        public bool UseLightProbes = true;

        public MeshRenderer Mr { get; private set; }

        void Awake() {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf.sharedMesh == null) {
                mf.sharedMesh = Resources.Load<Mesh>("DecalCube");
            }
            Mr = GetComponent<MeshRenderer>();
            Mr.shadowCastingMode = ShadowCastingMode.Off;
            Mr.receiveShadows = false;
            Mr.materials = new Material[] { };
            Mr.lightProbeUsage = LightProbeUsage.BlendProbes;
            Mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        void OnWillRenderObject() {
            if (Camera.current == null || DecalRenderer.Main == null) {
                return;
            }
            if (!DecalRenderer.Main.isActiveAndEnabled) {
                return;
            }
            if (Fade <= 0.0f) {
                return;
            }
            if (Material == null) {
                return;
            }
            Material.enableInstancing = DecalRenderer.Main.UseInstancing;
            DecalRenderer.Main.Add(this, LimitTo);
        }

#if UNITY_EDITOR
        void OnDrawGizmos() {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.clear;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);

            Gizmos.color = Color.white * 0.2f;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        void OnDrawGizmosSelected() {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white * 0.5f;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
#endif

        
    }
}