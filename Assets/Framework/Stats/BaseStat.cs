using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PixelComrades {
    [System.Serializable]
    public class BaseStat : IDisposable {

        public BaseStat() {}

        public BaseStat(int entity, string label, float baseValue) {
            _entity = entity;
            _baseValue = baseValue;
            _id = label;
            _label = label;
        }

        public BaseStat(int entity, string label, string id, float baseValue) {
            _entity = entity;
            _baseValue = baseValue;
            _id = id;
            _label = label;
        }

        public BaseStat(int entity, float baseValue, string label, float maxBaseValue) {
            _entity = entity;
            _baseValue = baseValue;
            _label = label;
            _id = label;
            _maxBaseValue = maxBaseValue;
        }

        [HideInInspector, SerializeField] private int _entity;
        [SerializeField] private string _label = "";
        [Range(0, 100), SerializeField] private float _baseValue = 0;
        [SerializeField] private string _id;
        [SerializeField] private float _maxBaseValue = 9999;

        public event Action<BaseStat> OnStatChanged;
        public event Action<BaseStat> OnStatReset;

        private List<StatValueMod> _valueMods = new List<StatValueMod>();
        private List<StatValueMod> _percentMods = new List<StatValueMod>();
        private List<Derived> _derivedStats = new List<Derived>();
        private float _modTotal;

        public Entity Entity { get { return EntityController.Get(_entity); } }
        public string ID { get { return _id; } }
        public string Label { get { return _label; } }
        public float BaseValue { get { return _baseValue; } protected set { _baseValue = value; } }
        public float ModTotal { get { return _modTotal; } }
        public float MaxBaseValue { get => _maxBaseValue; }
        public virtual float Value { get { return _baseValue + _modTotal; } }

        public virtual void AddToBase(float amountToAdd) {
            if (_baseValue >= _maxBaseValue) {
                return;
            }
            _baseValue += amountToAdd;
            _baseValue = Mathf.Clamp(_baseValue, -_maxBaseValue, _maxBaseValue);
            StatChanged();
        }

        public virtual void ChangeBase(float newBase) {
            _baseValue = newBase;
            _baseValue = Mathf.Clamp(_baseValue, -_maxBaseValue, _maxBaseValue);
            StatChanged();
        }

        public void Dispose() {
            ClearMods();
            ClearListeners();
        }

        public override string ToString() {
            return string.Format("{0}: {1:F0}", Label, Value);
        }

        public virtual string ToLabelString() {
            return Label.ToBoldLabel(Value.ToString("F0"));
        }

        public string AddValueMod(float amount) {
            var mod = new StatValueMod(amount);
            _valueMods.Add(mod);
            CalcModValue();
            return mod.Id;
        }

        public void AddValueMod(StatValueMod mod) {
            _valueMods.Add(mod);
            CalcModValue();
        }

        public string AddPercentMod(float normalizedAmt, string id = "") {
            normalizedAmt = normalizedAmt.CheckNormalized();
            var mod = new StatValueMod(normalizedAmt, id);
            _percentMods.Add(mod);
            CalcModValue();
            return mod.Id;
        }

        public void RemoveMod(string id) {
            for (int i = 0; i < _valueMods.Count; i++) {
                if (_valueMods[i].Id == id) {
                    _valueMods.RemoveAt(i);
                    CalcModValue();
                    return;
                }
            }
            for (int i = 0; i < _percentMods.Count; i++) {
                if (_percentMods[i].Id == id) {
                    _percentMods.RemoveAt(i);
                    CalcModValue();
                    return;
                }
            }
        }

        public bool HasMod(string id) {
            for (int i = 0; i < _valueMods.Count; i++) {
                if (_valueMods[i].Id == id) {
                    return true;
                }
            }
            for (int i = 0; i < _percentMods.Count; i++) {
                if (_percentMods[i].Id == id) {
                    return true;
                }
            }
            return false;
        }

        public string AddDerivedStat(float percent, BaseStat targetStat, string id = "") {
            if (targetStat == null) {
                return "";
            }
            var mod = new DerivedStat(percent, targetStat, id);
            mod.UpdateValue(Value);
            _derivedStats.Add(mod);
            return mod.Id;
        }

        public void RemoveDerivedStat(string id) {
            for (int i = 0; i < _derivedStats.Count; i++) {
                if (_derivedStats[i].Id == id) {
                    _derivedStats[i].Remove();
                    _derivedStats.RemoveAt(i);
                    return;
                }
            }
        }

        public void UpdateDerivedStat(string id, float percent) {
            for (int i = 0; i < _derivedStats.Count; i++) {
                if (_derivedStats[i].Id == id) {
                    _derivedStats[i].UpdatePercent(percent);
                    return;
                }
            }
        }

        public void ClearDerivedStats() {
            for (int i = 0; i < _derivedStats.Count; i++) {
                _derivedStats[i].Remove();
            }
            _derivedStats.Clear();
        }

        public void UpdateModValue(string id, float value) {
            for (int i = 0; i < _valueMods.Count; i++) {
                if (_valueMods[i].Id == id) {
                    _valueMods[i].Value = value;
                    CalcModValue();
                    return;
                }
            }
            for (int i = 0; i < _percentMods.Count; i++) {
                if (_percentMods[i].Id == id) {
                    _percentMods[i].Value = value;
                    CalcModValue();
                    return;
                }
            }
        }

        public void ClearMods() {
            _valueMods.Clear();
            _percentMods.Clear();
            CalcModValue();
            ClearDerivedStats();
        }

        public void Reset() {
            ClearMods();
            OnStatReset?.Invoke(this);
        }

        public void ClearListeners() {
            OnStatChanged = null;
            OnStatReset = null;
        }

        public string DebugStatMods() {
            StringBuilder tester = new StringBuilder();
            for (int i = 0; i < _derivedStats.Count; i++) {
                tester.Append(" derived " + _derivedStats[i].TargetDebug() + " " + _derivedStats[i].ValueTotal + " /");
            }
            for (int i = 0; i < _valueMods.Count; i++) {
                tester.Append(" mod " + i + " " + _valueMods[i].Value + " /");
            }
            return tester.ToString();
        }

        protected virtual void CalcModValue() {
            _modTotal = 0;
            for (int i = 0; i < _valueMods.Count; i++) {
                _modTotal += _valueMods[i].Value;
            }
            for (int i = 0; i < _percentMods.Count; i++) {
                _modTotal += _percentMods[i].Value * _baseValue;
            }
            CheckDerived();
            if (OnStatChanged != null) {
                OnStatChanged(this);
            }
        }

        protected void CheckDerived() {
            for (int i = 0; i < _derivedStats.Count; i++) {
                _derivedStats[i].UpdateValue(Value);
            }
        }

        protected virtual void StatChanged() {
            CalcModValue();
            if (OnStatChanged != null) {
                OnStatChanged(this);
            }
        }

        public class StatValueMod {
            public string Id;
            public float Value;

            public StatValueMod(float value) {
                Value = value;
                Id = System.Guid.NewGuid().ToString();
            }

            public StatValueMod(float value, string id) {
                Value = value;
                Id = string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id;
            }
        }

        public abstract class Derived {

            private float _value;
            private float _percent;
            private string _id;
            public float ValueTotal { get { return _value * _percent; } }

            public string Id {  get { return _id; } protected set { _id = value; } }

            protected abstract void UpdateTarget();
            public abstract string TargetDebug();
            public abstract void Remove();

            protected Derived(float percent) {
                _value = float.MinValue;
                _percent = percent.CheckNormalized();
            }

            public void UpdatePercent(float newValue) {
                _percent = newValue.CheckNormalized();
                UpdateTarget();
            }

            public void UpdateValue(float newValue) {
                if (Math.Abs(newValue - _value) < 0.0001f) {
                    return;
                }
                _value = newValue;
                UpdateTarget();
            }
        }

        public class DerivedStat : Derived {

            public BaseStat TargetStat { get; private set; }
            
            public DerivedStat(float percent, BaseStat stat, string id = "") : base(percent) {
                TargetStat = stat;
                if (string.IsNullOrEmpty(id)) {
                    Id = TargetStat.AddValueMod(ValueTotal);
                }
                else {
                    Id = id;
                    TargetStat.AddValueMod(new StatValueMod(ValueTotal, id));
                }
            }

            public override string TargetDebug() {
                return TargetStat.Label;
            }

            protected override void UpdateTarget() {
                TargetStat.UpdateModValue(Id, ValueTotal);
            }

            public override void Remove() {
                TargetStat.RemoveMod(Id);
            }
        }

        public class DerivedGeneric : Derived {

            private Action<float> _targetDel;
            private Action<string> _onRemoveDel;

            public DerivedGeneric(float percent, Action<float> del, Action<string> onRemove, string id) : base(percent) {
                Id = id;
                _targetDel = del;
                _targetDel?.Invoke(ValueTotal);
                _onRemoveDel = onRemove;
            }

            public override string TargetDebug() {
                return _targetDel != null ? _targetDel.Target.ToString() : "null";
            }

            protected override void UpdateTarget() {
                _targetDel?.Invoke(ValueTotal);
            }

            public override void Remove() {
                _onRemoveDel?.Invoke(Id);
            }
        }
    }
}