using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {
    [Serializable]
    public class TagsComponent {

        private Entity _entity;

        public int Owner { get { return _entity.Id; } set { } }

        private int[] _tags = new int[EntityTags.MAX_TAGS_LIMIT];

        public void Dispose() {
            _tags = null;
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
                    Debug.LogFormat("{0}:{1}",i, _tags[i]);
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
        /// <summary>
        /// id must be from EntityTags
        /// </summary>
        /// <param name="id"></param>
        public void Add(int id) {
            _tags[id]++;
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
            _tags[id] = Math.Max(_tags[id] - 1, 0);
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

        public TagsComponent(Entity owner) {
            _entity = owner;
        }
    }
}