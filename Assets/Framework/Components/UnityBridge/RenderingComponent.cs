using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.Rendering;

namespace PixelComrades {
    [System.Serializable]
	public sealed class RenderingComponent : IComponent {

        private CachedGenericComponent<IRenderingComponent> _component;
        public IRenderingComponent Rendering { get { return _component.Value; } }

        public RenderingComponent(IRenderingComponent rendering) {
            _component = new CachedGenericComponent<IRenderingComponent>(rendering);
        }

        public void Set(IRenderingComponent rendering) {
            _component.Set(rendering);
        }

        public void Clear() {
            _component.Clear();
        }

        public MaterialPropertyBlock[] GetMatBlocks { get { return Rendering.MaterialBlocks; } }
        
        public void ApplyMaterialBlocks(MaterialPropertyBlock[] matBlocks) {
            Rendering.ApplyMaterialBlocks(matBlocks);
        }

        public void SetVisible(RenderingMode status) {
            Rendering.SetRendering(status);
        }

        public RenderingComponent(SerializationInfo info, StreamingContext context) {
            _component = info.GetValue(nameof(_component), _component);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_component), _component);
        }
    }

    public interface IRenderingComponent {
        MaterialPropertyBlock[] MaterialBlocks { get; }
        void ApplyMaterialBlocks(MaterialPropertyBlock[] matBlocks);
        void SetRendering(RenderingMode status);
    }

    public enum RenderingMode {
        None,
        Normal,
        NoShadows,
        ShadowsOnly,
    }

    public interface IWeaponModel {
        Transform Tr { get; }
        Transform Spawn { get; }
        MusclePose IdlePose { get; }
        void SetFx(bool status);
        void Setup();
    }

    public static class RenderingComponentExtensions {
        public static void SetMode(this Renderer renderer, RenderingMode status) {
            renderer.enabled = status != RenderingMode.None;
            switch (status) {
                case RenderingMode.Normal:
                    renderer.shadowCastingMode = ShadowCastingMode.On;
                    break;
                case RenderingMode.ShadowsOnly:
                    renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                    break;
                case RenderingMode.NoShadows:
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    break;
            }
        }
    }
}
