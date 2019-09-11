using System;
using UnityEngine;
using System.Collections;

namespace PixelComrades {
    [System.Serializable] public class VitalStat : BaseStat {

        public VitalStat() {}

        public VitalStat(string label, string id, float baseValue, float recovery) : base(label, id, baseValue) {
            _recoveryPercent = new BaseStat("RecoverRate", recovery);
        }

        private float _current;
        private bool _canRecover = true;

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
        public bool CanRecover => _canRecover;

        private BaseStat _recoveryPercent;
        public BaseStat RecoveryPercent { get { return _recoveryPercent; } }

        public void DoRecover(float mod = 1) {
            if (_recoveryPercent != null && _current < Value && _recoveryPercent.Value > 0) {
                _current += (_recoveryPercent.Value * Value) * mod;
                StatChanged();
            }
        }

        public void ClearRecovery(bool canRecover) {
            _recoveryPercent = null;
            _canRecover = canRecover;
        }

        public void SetCurrentFromStat(BaseStat stat) {
            Current = stat.Value;
        }

        public void AddValueMultiToCurrent(float overload) {
            if (!_canRecover) {
                return;
            }
            _current += Value * overload;
            StatChanged();
        }

        //public void ReducePercent(float percentNorm) {
        //    Current -= Value * percentNorm;
        //}

        //public void SetRecoverPercent(float percent) {
        //    Recovery.ChangeBase(BaseValue * percent);
        //}

        public override string ToString() {
            return string.Format("{0}: {1:F0}/{2:F0}", Label, Current, Value);
        }

        public override string ToLabelString() {
            return string.Format("<b>{0}:</b> {1:F0}/{2:F0}", Label, _current, Value);

        }

        public void SetMax() {
            if (!_canRecover) {
                return;
            }
            Current = Value;
        }

        //protected override void CalcModValue() {
        //    base.CalcModValue();
        //    _current = Mathf.Clamp(_current, 0, Value);
        //}
    }
}