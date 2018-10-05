using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CachedStat<T> where T : BaseStat, IDisposable {

        private T _stat = null;
        private Entity _entity;
        private string _statId;

        public CachedStat(Entity entity, string statId) {
            _entity = entity;
            _statId = statId;
            _stat = _entity.Stats.Get<T>(_statId);
        }

        public CachedStat(){}

        public void Set(Entity entity, string statId) {
            _entity = entity;
            _statId = statId;
            _stat = _entity.Stats.Get<T>(_statId);
        }

        public T Stat {
            get {
                if (_stat == null) {
                    _stat = _entity?.Stats.Get<T>(_statId);
                }
                return _stat;
            }
        }

        public float Value => Stat?.Value ?? 0;

        public void Dispose() {
            _entity = null;
            _stat = null;
        }
    }
}
