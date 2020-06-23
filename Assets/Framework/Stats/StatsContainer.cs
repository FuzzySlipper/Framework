using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class StatsContainer : IComponent, IDisposable {

        private Dictionary<string, BaseStat> _dict = new Dictionary<string, BaseStat>();
        private Dictionary<string, VitalStat> _vitals = new Dictionary<string, VitalStat>();
        private List<BaseStat> _list = new List<BaseStat>();

        public StatsContainer() {}

        public StatsContainer(SerializationInfo info, StreamingContext context) {
            var stats = (List<BaseStat>) info.GetValue("Stats", typeof(List<BaseStat>));
            var vitals = (List<VitalStat>) info.GetValue("Vitals", typeof(List<VitalStat>));
            for (int i = 0; i < stats.Count; i++) {
                Add(stats[i]);
            }
            for (int i = 0; i < vitals.Count; i++) {
                Add(vitals[i]);
            }
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            var baseStats = new List<BaseStat>();
            var vitalStats = new List<VitalStat>();
            for (int i = 0; i < _list.Count; i++) {
                if (_list[i] is VitalStat stat) {
                    vitalStats.Add(stat);
                }
                else {
                    baseStats.Add(_list[i]);
                }
            }
            info.AddValue("Stats", baseStats);
            info.AddValue("Vitals", vitalStats);
        }

        public BaseStat this[int index] { get { return _list[index]; } }
        public int Count { get { return _list.Count; } }
        public void Add(BaseStat item) {
            if (item == null || _dict.ContainsKey(item.ID)) {
                return;
            }
            _list.Add(item);
            _dict.Add(item.ID, item);
            if (item is VitalStat vital) {
                _vitals.Add(vital.ID, vital);
            }
        }

        public void AddRange<T>(IList<T> values) where T : BaseStat {
            if (values == null) {
                return;
            }
            for (int i = 0; i < values.Count; i++) {
                Add(values[i]);
            }
        }

        public void Remove(BaseStat item) {
            _list.Remove(item);
            _dict.Remove(item.ID);
            if (item is VitalStat vital) {
                _vitals.Remove(vital.ID);
            }
        }

        public BaseStat Get(string id) {
            return _dict.TryGetValue(id, out var stat) ? stat : null;
        }

        public bool HasStat(string id) {
            return _dict.ContainsKey(id);
        }

        public BaseStat GetOrAdd(string id, string label = "") {
            if (_dict.TryGetValue(id, out var stat)) {
                return stat;
            }
            var value = 0f;
            var fakeEnum = GameData.Enums.GetEnumIndex(id, out var index);
            if (fakeEnum != null) {
                if (string.IsNullOrEmpty(label)) {
                    label = fakeEnum.GetNameAt(index);
                }
                value = fakeEnum.GetAssociatedValue(index);
            }
            if (string.IsNullOrEmpty(label)) {
                label = id;
            }
            stat = new BaseStat(this.GetEntity(), label, id, value);
            Add(stat);
            return stat;
        }

        public T Get<T>(string id) where T : BaseStat {
            return _dict.TryGetValue(id, out var stat) ? stat as T : null;
        }

        public VitalStat GetVital(string id) {
            return _vitals.TryGetValue(id, out var stat) ? stat: null;
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

        public string DebugStats() {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var stat in _dict) {
                sb.AppendNewLine(stat.Value.ToString());
            }
            Debug.Log(sb.ToString());
            return sb.ToString();
        }

        public void SetMax() {
            foreach (var vital in _vitals) {
                vital.Value.SetMax();
            }
        }

        public void DoRecovery(float mod) {
            foreach (var vital in _vitals) {
                vital.Value.DoRecover(mod);
            }
        }

        public void Clear() {
            ClearMods();
            _dict.Clear();
            _vitals.Clear();
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

        public void Dispose() {
            Clear();
        }
    }
}
