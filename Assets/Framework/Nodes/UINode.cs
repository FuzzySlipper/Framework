using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class UINode : INode {
        public Entity Entity;
        public CachedComponent<TransformComponent> Tr;
        public CachedComponent<ModelComponent> Model;
        public CachedComponent<LabelComponent> Label;
        public CachedComponent<DescriptionComponent> Description;
        public CachedComponent<DataDescriptionComponent> DataDescription;
        public CachedComponent<IconComponent> Icon;


        private CachedComponent<RotationComponent> _rotation;
        private CachedComponent<PositionComponent> _position;

        public UINode(Entity entity, Dictionary<System.Type, ComponentReference> list) {
            Entity = entity;
            Tr = new CachedComponent<TransformComponent>(entity, list);
            Description = new CachedComponent<DescriptionComponent>(entity, list);
            DataDescription = new CachedComponent<DataDescriptionComponent>(entity, list);
            Icon = new CachedComponent<IconComponent>(entity, list);
            Label = new CachedComponent<LabelComponent>(entity, list);
            Model = new CachedComponent<ModelComponent>(entity, list);
            _position = new CachedComponent<PositionComponent>(entity, list);
            _rotation = new CachedComponent<RotationComponent>(entity, list);
        }

        public void Setup(GameObject obj) {
            Model.c.Model = obj.GetComponent<ModelWrapper>();
            Tr.Assign(new TransformComponent(obj.transform));
        }

        public void Clear() {
            Model.c.Model = null;
            Tr.Assign(new TransformComponent(null));
        }

        public Vector3 position { get { return Tr.c.Tr?.position ?? _position.c?.Position ?? Vector3.zero; } }
        public Quaternion rotation { get { return Tr.c.Tr?.rotation ?? _rotation.c?.Rotation ?? Quaternion.identity; } }

        public void Dispose() {
            Tr.Dispose();
            Tr = null;
            Model.Dispose();
            Model = null;
            Label.Dispose();
            Label = null;
            _position.Dispose();
            _position = null;
            _rotation.Dispose();
            _rotation = null;
            Icon.Dispose();
            Icon = null;
            Description.Dispose();
            Description = null;
            DataDescription.Dispose();
            DataDescription = null;
        }

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(DescriptionComponent),
                typeof(LabelComponent),
                typeof(TransformComponent),
            };
        }
    }
}
