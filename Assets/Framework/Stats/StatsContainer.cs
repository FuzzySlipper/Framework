using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Low)]
    public class StatsContainer : IReceive<DamageEvent>, IReceive<HealEvent> {

        public Entity Owner { get; private set; }

        private Dictionary<string, BaseStat> _dict = new Dictionary<string, BaseStat>();
        private Dictionary<string, VitalStat> _vitals = new Dictionary<string, VitalStat>();
        protected List<BaseStat> List = new List<BaseStat>();

        public StatsContainer(Entity owner) {
            Owner = owner;
        }

        public BaseStat this[int index] { get { return List[index]; } }
        public int Count { get { return List.Count; } }
        public VitalStat HealthStat { get; private set; }

        public void Add(BaseStat item) {
            if (item == null || _dict.ContainsKey(item.ID)) {
                return;
            }
            List.Add(item);
            _dict.Add(item.ID, item);
            if (item is VitalStat vital) {
                if (item.ID == GameOptions.Get(RpgSettings.HealthStat, "Vitals.Health")) {
                    HealthStat = vital;
                }
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
            List.Remove(item);
            _dict.Remove(item.ID);
            if (item is VitalStat vital) {
                if (item.ID == GameOptions.Get(RpgSettings.HealthStat, "Vitals.Health")) {
                    HealthStat = null;
                }
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
            stat = new BaseStat(label, id, value);
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

        public void Handle(DamageEvent msg) {
            var damage = msg.Amount;
            if (damage <= 0) {
                return;
            }
            if (_vitals.TryGetValue(msg.TargetVital, out var vital)) {
                vital.Current -= msg.Amount;
            }
            else if (_vitals.TryGetValue(GameData.Vitals.GetID(msg.TargetVital), out vital)) {
                vital.Current -= msg.Amount;
            }
            if (msg.Amount > 0) {
                Owner.Post(new CombatStatusUpdate(msg.Amount.ToString("F1"), Color.red));
            }
        }

        public void Handle(HealEvent msg) {
            if (_vitals.TryGetValue(msg.TargetVital, out var vital)) {
                vital.Current += msg.Amount;
            }
            else if (_vitals.TryGetValue(GameData.Vitals.GetID(msg.TargetVital), out vital)) {
                vital.Current += msg.Amount;
            }
            if (msg.Amount > 0) {
                Owner.Post(new CombatStatusUpdate(msg.Amount.ToString("F1"), Color.green));
            }
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
            HealthStat = null;
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
