using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ModelComponent : IComponent {
        public int Owner { get; set; }

        public IModelComponent Model;

        public ModelComponent(IModelComponent model) {
            Model = model;
        }

        public MaterialPropertyBlock[] GetMatBlocks { get { return Model.MaterialBlocks; } }
        public Renderer[] GetRenderers { get { return Model.Renderers; } }
        
        public void ApplyMaterialBlocks(MaterialPropertyBlock[] matBlocks) {
            Model.ApplyMaterialBlocks(matBlocks);
        }

        public void SetVisible(bool status) {
            Model.SetVisible(status);
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
