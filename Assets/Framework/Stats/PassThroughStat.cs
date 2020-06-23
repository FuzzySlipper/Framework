using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    public class PassThroughStat : BaseStat {
        
        private StringBuilder _sb = new StringBuilder();
        
        private readonly List<BaseStat> _extraStats = new List<BaseStat>();

        public PassThroughStat() {
        }

        public PassThroughStat(int entity, string label, float baseValue) : base(entity, label, baseValue) {
        }

        public PassThroughStat(int entity, string label, string id, float baseValue) : base(entity, label, id, baseValue) {
        }

        public PassThroughStat(int entity, float baseValue, string label, float maxBaseValue) : base(entity, baseValue, label, maxBaseValue) {
        }

        public void AddStat(BaseStat stat) {
            _extraStats.Add(stat);
        }

        public void SetStat(BaseStat stat) {
            _extraStats.Clear();
            _extraStats.Add(stat);
        }

        public void ClearStats() {
            _extraStats.Clear();
        }
        

        public override float Value {
            get {
                var total = BaseClassValue;
                for (int i = 0; i < _extraStats.Count; i++) {
                    total += _extraStats[i].Value;
                }
                return total;
            }
        }

        public override string ToString() {
            _sb.Clear();
            for (int i = 0; i < _extraStats.Count; i++) {
                _sb.NewLineAppend(_extraStats[i].ToString());
            }
            _sb.Append(base.ToString());
            return _sb.ToString();
        }

        public override string ToLabelString() {
            _sb.Clear();
            for (int i = 0; i < _extraStats.Count; i++) {
                _sb.NewLineAppend(_extraStats[i].ToLabelString());
            }
            _sb.Append(base.ToLabelString());
            return _sb.ToString();
        }
    }
}
