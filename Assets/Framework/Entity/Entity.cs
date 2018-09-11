using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {
    public class Entity : IEquatable<Entity> {

        public int Id;
        public string Name;
        public TagsComponent Tags;
        public int ParentId = -1;

        public void Init() {
            Tags = new TagsComponent(this);
            this.Add(Tags);
        }

        public void Destroy() {
            EntityController.RemoveEntity(this);
            Id = -1;
            Name = "Destroyed";
        }

        public bool IsDestroyed() {
            return Id < 0;
        }

        public static Entity New(string name) {
            var entity = EntityController.AddEntity(new Entity());
            entity.Name = name;
            entity.Init();
            return entity;
        }

        private Entity(){}

        public void ClearParent() {
            ParentId = -1;
        }

        public void ClearParent(int matchId) {
            if (ParentId == matchId) {
                ParentId = -1;
            }
        }

        public static implicit operator int(Entity reference) {
            if (reference == null) {
                return -1;
            }
            return reference.Id;
        }

        public override bool Equals(object obj) {
            if (obj is Entity entity) {
                return entity.Id == Id;
            }
            return false;
        }

        public bool Equals(Entity other) {
            return other != null && other.Id == Id;
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public static bool operator ==(Entity entity, Entity other) {
            if (object.ReferenceEquals(entity, null)) {
                return object.ReferenceEquals(other, null);
            }
            if (object.ReferenceEquals(other, null)) {
                return false;
            }
            return entity.Id == other.Id;
        }

        public static bool operator !=(Entity entity, Entity other) {
            return !(entity == other);
        }
    }
}