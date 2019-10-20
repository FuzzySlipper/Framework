using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IEntityTemplate {
        Entity Entity { get; }
        void Register(Entity entity);
        void Dispose();
        bool Disposed { get; }
        System.Type[] GetTypes();
    }

    public abstract class BaseTemplate : IEntityTemplate, IEquatable<Entity> {
        public Entity Entity { get; private set; }

        public abstract List<CachedComponent> GatherComponents { get; }
        public abstract Type[] GetTypes();

        public bool Disposed { get; private set; }

        public virtual void Register(Entity entity) {
            Entity = entity;
            Disposed = false;
            var components = GatherComponents;
            for (int i = 0; i < components.Count; i++) {
                components[i].Set(entity);
            }
        }

        public virtual void Dispose() {
            Disposed = true;
            var components = GatherComponents;
            for (int i = 0; i < components.Count; i++) {
                components[i].Dispose();
            }
        }

        public string GetName() {
            return Entity.GetRoot().Name;
        }

        public static bool operator ==(BaseTemplate entity, Entity other) {
            if (object.ReferenceEquals(entity, null)) {
                return object.ReferenceEquals(other, null);
            }
            if (object.ReferenceEquals(other, null)) {
                return false;
            }
            return entity.Entity?.Id == other.Id;
        }

        public static bool operator !=(BaseTemplate entity, Entity other) {
            return !(entity == other);
        }

        private bool Equals(BaseTemplate other) {
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
            return Equals((BaseTemplate) obj);
        }

        public override int GetHashCode() {
            return (Entity != null ? Entity.GetHashCode() : 0);
        }

        public static implicit operator Entity(BaseTemplate reference) {
            if (reference == null) {
                return null;
            }
            return reference.Entity;
        }

        public T Get<T>() where T : IComponent {
            return Entity.Get<T>();
        }
    }

    public static class TemplateExtensions {

        public static bool IsPlayer(this IEntityTemplate entityTemplate) {
            return entityTemplate.Entity.Tags.Contain(EntityTags.Player);
        }
        
//        public static T Find<T>(this IEntityTemplate entityTemplate) where T : IComponent {
//            return entityTemplate.Entity.Find<T>();
//        }
    }
}
