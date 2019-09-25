using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class UINode : BaseNode {
        
        private CachedComponent<RenderingComponent> _rendering = new CachedComponent<RenderingComponent>();
        private CachedComponent<LabelComponent> _label = new CachedComponent<LabelComponent>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<DescriptionComponent> _description = new CachedComponent<DescriptionComponent>();
        private CachedComponent<DataDescriptionComponent> _dataDescription = new CachedComponent<DataDescriptionComponent>();
        private CachedComponent<IconComponent> _icon = new CachedComponent<IconComponent>();

        public TransformComponent Tr { get => _tr.Value; }
        public LabelComponent Label => _label.Value;
        public DescriptionComponent Description { get => _description; }
        public IconComponent Icon { get => _icon; }
        public RenderingComponent Rendering { get => _rendering; }
        public DataDescriptionComponent DataDescription { get => _dataDescription; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _label, _rendering, _tr, _description, _dataDescription, _icon
        };

        public UINode() {
        }

        public void Setup(GameObject obj) {
            _rendering.Value.Set(obj.GetComponent<RenderingWrapper>());
            
        }

        public void Clear() {
            _rendering.Value.Clear();
            _tr.Value.Set(null);
        }

        public Vector3 position { get { return Tr != null ? Tr.position : Vector3.zero; } }
        public Quaternion rotation { get { return Tr != null ? Tr.rotation : Quaternion.identity; } }

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(DescriptionComponent),
                typeof(LabelComponent),
                typeof(IconComponent),
            };
        }
    }
}
