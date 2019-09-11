using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class CachedStat<T> : IDisposable, ISerializable where T : BaseStat {

        private T _stat = null;
        private int _entityId;
        private string _statId;

        public CachedStat(int entity, string statId) {
            _entityId = entity;
            _statId = statId;
            _stat = EntityController.Get(_entityId)?.Get<StatsContainer>()?.Get<T>(_statId);
        }

        public CachedStat(int entity, T stat) {
            _stat = stat;
            _entityId = entity;
            _statId = stat.ID;
        }

        public T Stat {
            get {
                if (_stat == null) {
                    var entity = EntityController.Get(_entityId);
                    if (entity != null) {
                        if (entity is Actor actor) {
                            _stat = actor.Stats.Get<T>(_statId);                            
                        }
                        else {
                            _stat = entity.Get<StatsContainer>()?.Get<T>(_statId);
                        }
                    }
                }
                return _stat;
            }
        }

        public float Value => Stat?.Value ?? 0;

        public void Dispose() {
            _stat = null;
        }

        public static implicit operator T(CachedStat<T> reference) {
            return reference.Stat;
        }

        public CachedStat(SerializationInfo info, StreamingContext context) {
            _entityId = info.GetValue(nameof(_entityId), _entityId);
            _statId = info.GetValue(nameof(_statId), _statId);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_entityId), _entityId);
            info.AddValue(nameof(_statId), _statId);
        }
    }
}
