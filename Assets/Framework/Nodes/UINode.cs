using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class UINode : BaseNode {
        
        private CachedComponent<ModelComponent> _model = new CachedComponent<ModelComponent>();
        private CachedComponent<LabelComponent> _label = new CachedComponent<LabelComponent>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<DescriptionComponent> _description = new CachedComponent<DescriptionComponent>();
        private CachedComponent<DataDescriptionComponent> _dataDescription = new CachedComponent<DataDescriptionComponent>();
        private CachedComponent<IconComponent> _icon = new CachedComponent<IconComponent>();

        public Transform Tr { get => _tr.Value; }
        public LabelComponent Label => _label.Value;
        public DescriptionComponent Description { get => _description; }
        public IconComponent Icon { get => _icon; }
        public ModelComponent Model { get => _model; }
        public DataDescriptionComponent DataDescription { get => _dataDescription; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _label, _model, _tr, _description, _dataDescription, _icon
        };

        public UINode(Entity entity, SortedList<System.Type, ComponentReference> list) {
            Register(entity, list);
        }

        public UINode() {
        }

        public void Setup(GameObject obj) {
            _model.Value.Set(obj.GetComponent<ModelWrapper>());
            
        }

        public void Clear() {
            _model.Value.Clear();
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
