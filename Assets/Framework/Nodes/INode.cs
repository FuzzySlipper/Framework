using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface INode {
        void Register(Entity entity, Dictionary<System.Type, ComponentReference> list);
        void Dispose();
    }

    public abstract class BaseNode : INode {
        public Entity Entity { get; private set; }

        public abstract List<CachedComponent> GatherComponents { get; }

        public void Register(Entity entity, Dictionary<System.Type, ComponentReference> list) {
            Entity = entity;
            var components = GatherComponents;
            for (int i = 0; i < components.Count; i++) {
                components[i].Set(entity, list);
            }
        }

        public void Dispose() {
            var components = GatherComponents;
            for (int i = 0; i < components.Count; i++) {
                components[i].Dispose();
            }
        }
    }
}
