using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface INode {
        void Register(Entity entity, Dictionary<Type, ComponentReference> list);
        void Dispose();
        bool Disposed { get; }
    }

    public abstract class BaseNode : INode, IEquatable<Entity> {
        public Entity Entity { get; private set; }

        public abstract List<CachedComponent> GatherComponents { get; }
        public bool Disposed { get; private set; }

        public void Register(Entity entity, Dictionary<Type, ComponentReference> list) {
            Entity = entity;
            Disposed = false;
            var components = GatherComponents;
            for (int i = 0; i < components.Count; i++) {
                components[i].Set(entity, list);
            }
        }

        public void Dispose() {
            Disposed = true;
            var components = GatherComponents;
            for (int i = 0; i < components.Count; i++) {
                components[i].Dispose();
            }
        }

        public string GetName() {
            return Entity.GetRoot().Name;
        }

        public static bool operator ==(BaseNode entity, Entity other) {
            if (object.ReferenceEquals(entity, null)) {
                return object.ReferenceEquals(other, null);
            }
            if (object.ReferenceEquals(other, null)) {
                return false;
            }
            return entity.Entity?.Id == other.Id;
        }

        public static bool operator !=(BaseNode entity, Entity other) {
            return !(entity == other);
        }

        private bool Equals(BaseNode other) {
            if (other == null) {
                return false;
            }
            return other.Entity?.Id == Entity?.Id;
        }

        public bool Equals(Entity other) {
            if (other == null) {
                return false;
            }
            return other.Id == Entity?.Id;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (obj.GetType() != this.GetType()) {
                return false;
            }
            return Equals((BaseNode) obj);
        }

        public override int GetHashCode() {
            return (Entity != null ? Entity.GetHashCode() : 0);
        }

        public static implicit operator Entity(BaseNode reference) {
            if (reference == null) {
                return null;
            }
            return reference.Entity;
        }

        public T Get<T>() where T : IComponent {
            return Entity.Get<T>();
        }
    }
}
