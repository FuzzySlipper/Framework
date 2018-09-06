using System;
using UnityEngine;
using System.Collections;

namespace PixelComrades {
    [System.Serializable] public class VitalStat : BaseStat {

        private const float RecoverMinimum = 2.5f;

        public VitalStat() {}

        public VitalStat(string label, string id, float baseValue) : base(label, id, baseValue) {}

        private float _current;

        public float Current {
            get { return _current; }
            set {
                if (Math.Abs(_current - value) < 0.001f) {
                    return;
                }
                _current = value;
                //_current = Mathf.Clamp(_current, 0, Value);
                StatChanged();
            }
        }

        public float CurrentPercent { get { return Current > 0 ? Current / Value : 0; } }

        public bool IsMax { get { return Current >= Value; } }

        private BaseStat _recover = new BaseStat("RecoverRate", RecoverMinimum);

        public BaseStat Recovery { get { return _recover; } }

        public void DoRecover(float mod = 1) {
            if (_current < Value) {
                Current += (_recover.Value) * mod;
            }
        }

        public void SetOverloadMulti(float overload) {
            _current = Value * overload;
            StatChanged();
        }

        //public void ReducePercent(float percentNorm) {
        //    Current -= Value * percentNorm;
        //}

        //public void SetRecoverPercent(float percent) {
        //    Recovery.ChangeBase(BaseValue * percent);
        //}

        public override string ToString() {
            return string.Format("<b>{0}:</b> {1}/{2}", Label, _current.ToString("F0"), Value.ToString("F0"));
        }

        public override string ToDebugString() {
            return string.Format("{0}: {1}/{2}", Label, Current, Value);
        }

        public void SetMax() {
            Current = Value;
        }

        //protected override void CalcModValue() {
        //    base.CalcModValue();
        //    _current = Mathf.Clamp(_current, 0, Value);
        //}
    }
}