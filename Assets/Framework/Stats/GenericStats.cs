using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class GenericStats : ComponentContainer<BaseStat> {

        private Dictionary<string, BaseStat> _dict = new Dictionary<string, BaseStat>();

        public GenericStats(BaseStat[] values) : base(values) {}

        public override void Add(BaseStat item) {
            base.Add(item);
            if (_dict.ContainsKey(item.Id)) {
                _dict[item.Id] = item;
            }
            else {
                _dict.Add(item.Id, item);
            }
        }

        public override void Remove(BaseStat item) {
            base.Remove(item);
            _dict.Remove(item.Id);
        }

        public BaseStat Get(string id) {
            return _dict.TryGetValue(id, out var stat) ? stat : null;
        }

        public float GetValue(string id) {
            return _dict.TryGetValue(id, out var stat) ? stat.Value : 0;
        }

        public bool GetValue(string id, out float value) {
            if (_dict.TryGetValue(id, out var stat)) {
                value = stat.Value;
                return true;
            }
            value = 0;
            return false;
        }

        public GenericStats Clone() {
            return new GenericStats(List.ToArray());
        }

        public void ClearMods() {
            for (int i = 0; i < Count; i++) {
                this[i].ClearMods();
            }
        }

        public void Reset() {
            for (int i = 0; i < Count; i++) {
                this[i].Reset();
            }
        }
    }
}
