using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class VisibleNode : INode {
        public Entity Entity;
        public CachedComponent<TransformComponent> Tr;
        public CachedComponent<RigidbodyComponent> Rb;
        public CachedComponent<ModelComponent> Model;
        public CachedComponent<LabelComponent> Label;

        private CachedComponent<RotationComponent> _rotation;
        private CachedComponent<PositionComponent> _position;

        public VisibleNode(Entity entity, Dictionary<System.Type, ComponentReference> list) {
            Entity = entity;
            Tr = new CachedComponent<TransformComponent>(entity, list);
            Rb = new CachedComponent<RigidbodyComponent>(entity, list);
            Model = new CachedComponent<ModelComponent>(entity, list);
            Label = new CachedComponent<LabelComponent>(entity, list);
            _position = new CachedComponent<PositionComponent>(entity, list);
            _rotation = new CachedComponent<RotationComponent>(entity, list);
        }

        public Vector3 position { get { return Tr.c.Tr?.position ?? _position.c?.Position ?? Vector3.zero; } }
        public Quaternion rotation { get { return Tr.c.Tr?.rotation ?? _rotation.c?.Rotation ?? Quaternion.identity; } }

        public void Dispose() {
            Tr.Dispose();
            Tr = null;
            Rb.Dispose();
            Rb = null;
            Model.Dispose();
            Model = null;
            Label.Dispose();
            Label = null;
            _position.Dispose();
            _position = null;
            _rotation.Dispose();
            _rotation = null;
        }

        public static System.Type[] GetTypes() {
            return new System.Type[]{ 
                typeof(TransformComponent), 
                typeof(RigidbodyComponent), 
                typeof(ModelComponent),
            };
        }
    }
}
