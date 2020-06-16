using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public abstract class StatModHolder {
        
        public abstract void Attach(BaseStat target);
        public abstract void Remove();
        public abstract void Restore();
        public abstract string ModID { get; }
        public abstract string StatID { get;}
        public abstract BaseStat TargetStat { get; }
    }

    [System.Serializable]
    public class BasicValueModHolder : StatModHolder {
        
        [SerializeField] private string _id;
        [SerializeField] private CachedStat<BaseStat> _targetStat;
        [SerializeField] private float _amount;
        
        public override string ModID { get => _id; }
        public override BaseStat TargetStat { get => _targetStat; }
        public override string StatID { get { return _targetStat != null ? _targetStat.Stat.ID : ""; } }

        public BasicValueModHolder(float amount) {
            _amount = amount;
        }

        public BasicValueModHolder(BaseStat stat, BaseStat.StatValueMod valueMod) {
            _targetStat = new CachedStat<BaseStat>(stat);
            _amount = valueMod.Value;
            _id = valueMod.Id;
        }

        public override void Attach(BaseStat target) {
            if (target == null) {
                return;
            }
            _targetStat = new CachedStat<BaseStat>(target);
            _id = target.AddValueMod(_amount);
        }

        public override void Remove() {
            if (_targetStat != null) {
                _targetStat.Stat.RemoveMod(_id);
            }
            _targetStat = null;
            _id = "";
        }

        public override void Restore() {
            Attach(_targetStat);
        }
    }

    [System.Serializable]
    public class BasicPercentModHolder : StatModHolder {
        [SerializeField] private string _id;
        [SerializeField] private CachedStat<BaseStat> _targetStat;
        [SerializeField] private float _percent;
        public override string ModID { get => _id; }
        public override BaseStat TargetStat { get => _targetStat; }
        public override string StatID { get { return _targetStat != null ? _targetStat.Stat.ID : ""; } }

        public BasicPercentModHolder(float percent) {
            _percent = percent;
        }

        public override void Attach(BaseStat target) {
            if (target == null) {
                return;
            }
            _targetStat = new CachedStat<BaseStat>(target);
            _id = target.AddPercentMod(_percent);
        }

        public override void Remove() {
            if (_targetStat != null) {
                _targetStat.Stat.RemoveMod(_id);
            }
            _targetStat = null;
            _id = "";
        }

        public override void Restore() {
            Attach(_targetStat);
        }
    }

    [System.Serializable]
    public class DerivedStatModHolder : StatModHolder {

        [SerializeField] private string _id;
        [SerializeField] private CachedStat<BaseStat> _sourceStat;
        [SerializeField] private CachedStat<BaseStat> _targetStat;
        [SerializeField] private float _percent;
        public override string ModID { get => _id; }
        public override BaseStat TargetStat { get => _targetStat; }
        public override string StatID { get { return _sourceStat != null ? _sourceStat.Stat.ID : ""; } }

        public DerivedStatModHolder(BaseStat source, float percent) {
            _sourceStat = new CachedStat<BaseStat>(source);
            _percent = percent;
        }

        public DerivedStatModHolder(BaseStat source, BaseStat target, float percent) {
            _sourceStat = new CachedStat<BaseStat>(source);
            _targetStat = new CachedStat<BaseStat>(target);
            _percent = percent;
            _id = _sourceStat.Stat.AddDerivedStat(_percent, target);
        }

        public override void Attach(BaseStat target) {
            if (target == null) {
                return;
            }
            _id = _sourceStat.Stat.AddDerivedStat(_percent, target);
        }

        public override void Remove() {
            if (_sourceStat == null) {
                return;
            }
            _sourceStat.Stat.RemoveDerivedStat(_id);
            _sourceStat = null;
        }

        public override void Restore() {
            Attach(_targetStat);
        }
    }
}
