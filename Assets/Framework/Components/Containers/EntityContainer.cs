﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class EntityContainer : ISerializable {

        private int _limit = -1;
        private List<int> _list = new List<int>();

        public EntityContainer(int limit = -1) {
            _limit = limit;
        }
        
        public EntityContainer(SerializationInfo info, StreamingContext context) {
            _limit = info.GetValue(nameof(_limit), _limit);
            _list = info.GetValue(nameof(_list), _list);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_limit), _limit);
            info.AddValue(nameof(_list), _list);
        }
        
        public bool IsFull { get { return _limit >= 0 && _list.Count >= _limit; } }
        public int Count { get { return _list.Count; } }
        public Entity this[int index] {
            get {
                if (!_list.HasIndex(index)) {
                    return null;
                }
                return EntityController.GetEntity(_list[index]);
            }
        }

        public bool Contains(Entity item) {
            return _list.Contains(item);
        }

        public int Add(Entity entity) {
            _list.Add(entity);
            return _list.Count-1;
        }

        public bool Remove(Entity entity) {
            if (!_list.Contains(entity)) {
                return false;
            }
            _list.Remove(entity);
            return true;
        }

        public void Clear() {
            _list.Clear();
        }

        public void Destroy() {
            for (int i = 0; i < Count; i++) {
                this[i].Destroy();
            }
            _list.Clear();
        }

        public bool ContainsType(string type) {
            for (int i = 0; i < Count; i++) {
                if (this[i].Get<TypeId>().Id == type) {
                    return true;
                }
            }
            return false;
        }
    }

    public interface IEntityContainer {
        bool IsFull { get; }
        Entity this[int index] { get; }
        Entity Owner { get; }
        int Count { get; }
        bool Contains(Entity item);
        /// <summary>
        /// Should only be used by ContainerSystem
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        int ContainerSystemAdd(Entity item);
        /// <summary>
        /// Should only be used by ContainerSystem
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        void ContainerSystemSet(Entity item, int index);
        bool Remove(Entity entity);
        void Clear();
    }

    public struct ContainerChanged : IEntityMessage {
        public IEntityContainer EntityContainer { get; }

        public ContainerChanged(IEntityContainer entityContainer) {
            EntityContainer = entityContainer;
        }
    }

    public struct ContainerStatusChanged : IEntityMessage {
        public IEntityContainer EntityContainer { get; }
        public Entity Entity { get; }

        public ContainerStatusChanged(IEntityContainer entityContainer, Entity entity) {
            EntityContainer = entityContainer;
            Entity = entity;
        }
    }
}
