using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class UINode : INode {
        public Entity Entity;
        public CachedComponent<TransformComponent> Tr = new CachedComponent<TransformComponent>();
        public CachedComponent<ModelComponent> Model = new CachedComponent<ModelComponent>();
        public CachedComponent<LabelComponent> Label = new CachedComponent<LabelComponent>();
        public CachedComponent<DescriptionComponent> Description = new CachedComponent<DescriptionComponent>();
        public CachedComponent<DataDescriptionComponent> DataDescription = new CachedComponent<DataDescriptionComponent>();
        public CachedComponent<IconComponent> Icon = new CachedComponent<IconComponent>();

        private CachedComponent<RotationComponent> _rotation = new CachedComponent<RotationComponent>();
        private CachedComponent<PositionComponent> _position = new CachedComponent<PositionComponent>();


        public UINode(Entity entity, Dictionary<System.Type, ComponentReference> list) {
            Register(entity, list);
        }

        public UINode() {
        }

        public void Register(Entity entity, Dictionary<System.Type, ComponentReference> list) {
            Entity = entity;
            Tr.Set(entity, list);
            Model.Set(entity, list);
            Label.Set(entity, list);
            Description.Set(entity, list);
            DataDescription.Set(entity, list);
            Icon.Set(entity, list);
            _position.Set(entity, list);
            _rotation.Set(entity, list);
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
            Model.Dispose();
            Label.Dispose();
            _position.Dispose();
            _rotation.Dispose();
            Icon.Dispose();
            Description.Dispose();
            DataDescription.Dispose();
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
