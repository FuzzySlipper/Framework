using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CachedStat<T> where T : BaseStat, IDisposable {

        private T _stat = null;
        private Entity _entity;
        private string _statId;
        private bool _isVital;

        public CachedStat(Entity entity, string statId) {
            _entity = entity;
            _statId = statId;
            _isVital = typeof(T) == typeof(VitalStat);
            TryGetStat();
        }

        public T Stat {
            get {
                if (_stat == null) {
                    TryGetStat();
                }
                return _stat;
            }
        }

        public float Value => Stat?.Value ?? 0;

        private void TryGetStat() {
            if (_isVital) {
                _entity.Get<GenericStats>(s => _stat = s.Get(_statId) as T);
            }
            else {
                _entity.Get<GenericStats>(s => _stat = s.Get(_statId) as T);
            }
        }

        public void Dispose() {
            _entity = null;
            _stat = null;
        }
    }
}
