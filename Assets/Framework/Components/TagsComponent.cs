using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {
    public sealed class TagsComponent : IDisposable {

        private Entity _entity;

        private int[] _tags = new int[EntityTags.MaxTagsLimit];
        public int[] Tags { get => _tags; }

        public TagsComponent() {}

        public void SetEntity(Entity entity) {
            _entity = entity;
        }

        public void ClearEntity() {
            _entity = null;
        }
//
//        public TagsComponent(SerializationInfo info, StreamingContext context) {
//            //_tags = info.GetValue(nameof(_tags), _tags);
//            var changed = (List<KeyValuePair<int, int>>) info.GetValue(nameof(_tags), typeof(List<KeyValuePair<int, int>>));
//            for (int i = 0; i < changed.Count; i++) {
//                _tags[changed[i].Key] = changed[i].Value;
//            }
//        }
//
//        public void GetObjectData(SerializationInfo info, StreamingContext context) {
//            List<KeyValuePair<int, int>> changedValues = new List<KeyValuePair<int, int>>();
//            for (int i = 0; i < _tags.Length; i++) {
//                if (_tags[i] != 0) {
//                    changedValues.Add(new KeyValuePair<int, int>(i, _tags[i]));
//                }
//            }
//            info.AddValue(nameof(_tags), changedValues);
//        }

        public void Dispose() {
            _entity = null;
        }

        public void Clear() {
            for (int i = 0; i < _tags.Length; i++) {
                _tags[i] = 0;
            }
        }

        public void DebugActive() {
            for (int i = 0; i < _tags.Length; i++) {
                if (_tags[i] > 0) {
                    Debug.LogFormat("{0}:{1}",i, EntityTags.GetNameAt(i));
                }
            }
        }

        public void Add(params int[] ids) {
            for (var i = 0; i < ids.Length; i++) {
                var index = ids[i];
                _tags[index]++;
            }
            _entity.Post(EntitySignals.TagsChanged);
        }

        public void Set(int id, int value) {
            _tags[id] = value;
            _entity.Post(EntitySignals.TagsChanged);
        }

        public void Replace(int[] tags) {
            _tags = tags;
        }

        /// <summary>
        /// id must be from EntityTags
        /// </summary>
        /// <param name="id"></param>
        public void Add(int id) {
            bool wasZero = _tags[id] == 0;
            _tags[id]++;
            if (!wasZero) {
                return;
            }
#if DEBUG
            DebugLog.Add(_entity.DebugId + " tag added " + EntityTags.GetNameAt(id));
#endif
            _entity.Post(EntitySignals.TagsChanged);
        }

        public void AddWithRoot(int id) {
            Add(id);
            var parent = _entity.GetRoot();
            if (parent != null) {
                parent.Tags.Add(id);
            }
        }

        public void AddWithParent(int id) {
            Add(id);
            var parent =_entity.GetParent();
            if (parent != null) {
                parent.Tags.Add(id);
            }
        }

        public bool Contain(int val) {
            return _tags[val] > 0;
        }

        public bool ContainWithParent(int val) {
            var parent = _entity.GetParent();
            if (parent == null) {
                return _tags[val] > 0;
            }
            return _tags[val] > 0 || parent.Tags.Contain(val);
        }

        public bool ContainAll(params int[] filter) {
            for (var i = 0; i < filter.Length; i++) {
                if (_tags[filter[i]] <= 0) {
                    return false;
                }
            }
            return true;
        }

        public bool ContainAny(params int[] filter) {
            for (var i = 0; i < filter.Length; i++) {
                if (_tags[filter[i]] > 0) {
                    return true;
                }
            }
            return false;
        }

        public void Remove(params int[] ids) {
            for (var i = 0; i < ids.Length; i++) {
                var index = ids[i];
                _tags[index] = Math.Max(_tags[index] - 1, 0);
            }
            _entity.Post(EntitySignals.TagsChanged);
        }

        public void Remove(int id) {
            bool wasZero = _tags[id] == 0;
            _tags[id] = Math.Max(_tags[id] - 1, 0);
            if (wasZero || _tags[id] > 0) {
                return;
            }
#if DEBUG
            DebugLog.Add(_entity.DebugId + " tag removed " + EntityTags.GetNameAt(id));
#endif
            _entity.Post(EntitySignals.TagsChanged);
        }

        public void RemoveWithRoot(int id) {
            Remove(id);
            var parent = _entity.GetRoot();
            if (parent != null) {
                parent.Tags.Remove(id);
            }
        }

        public void RemoveWithParent(int id) {
            Add(id);
            var parent = _entity.GetParent();
            if (parent != null) {
                parent.Tags.Remove(id);
            }
        }

        public bool IsConfused { get { return Contain(EntityTags.IsConfused); } }
        public bool IsSlowed { get { return Contain(EntityTags.IsSlowed); } }
        public bool IsStunned { get { return Contain(EntityTags.IsStunned); } }
    }
}