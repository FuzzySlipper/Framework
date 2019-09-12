using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class ModelComponent : IComponent {

        private CachedGenericComponent<IModelComponent> _component;
        public IModelComponent Model { get { return _component.Component; } }

        public ModelComponent(IModelComponent model) {
            _component = new CachedGenericComponent<IModelComponent>(model);
        }

        public void Set(IModelComponent model) {
            _component.Set(model);
        }

        public void Clear() {
            _component.Clear();
        }

        public MaterialPropertyBlock[] GetMatBlocks { get { return Model.MaterialBlocks; } }
        public Renderer[] GetRenderers { get { return Model.Renderers; } }
        
        public void ApplyMaterialBlocks(MaterialPropertyBlock[] matBlocks) {
            Model.ApplyMaterialBlocks(matBlocks);
        }

        public void SetVisible(bool status) {
            Model.SetVisible(status);
        }

        public ModelComponent(SerializationInfo info, StreamingContext context) {
            _component = info.GetValue(nameof(_component), _component);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_component), _component);
        }
    }

    public interface IModelComponent {
        Transform Tr { get; }
        MaterialPropertyBlock[] MaterialBlocks { get; }
        Renderer[] Renderers { get; }
        void ApplyMaterialBlocks(MaterialPropertyBlock[] matBlocks);
        void SetVisible(bool status);
    }

    public interface IWeaponModel {
        Transform Tr { get; }
        Transform Spawn { get; }
        MusclePose IdlePose { get; }
        void SetFx(bool status);
        void Setup();
    }
}
