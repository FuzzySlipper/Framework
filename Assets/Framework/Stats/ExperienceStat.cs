using System;
using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class ExperienceStat : ISerializable {

        public event System.Action OnLevelUp;

        private static GameOptions.CachedFloat _extraAdd = new GameOptions.CachedFloat("ExtraXpOffset");
        private static GameOptions.CachedFloat _multiplier = new GameOptions.CachedFloat("XpLevelMultiplier");

        public ExperienceStat() {
            SetNextXp();
            _experience.OnResourceChanged += CheckLevel;
        }

        public ExperienceStat(SerializationInfo info, StreamingContext context) {
            _level = (int) info.GetValue("Level", typeof(int));
            _nextLevelXp = _lastLevelXp = (float) info.GetValue("LastLevelXp", typeof(float));
            SetNextXp();
            _experience.ChangeValue((float) info.GetValue("Xp", typeof(float)));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Level", _level, typeof(int));
            info.AddValue("LastLevelXp", _lastLevelXp, typeof(float));
            info.AddValue("Xp", _experience.Value, typeof(float));
        }

        private FloatValueHolder _experience = new FloatValueHolder();
        private int _level;
        private float _nextLevelXp = -1;
        private float _lastLevelXp;

        private void CheckLevel() {
            if (_experience.Value <= _nextLevelXp) {
                return;
            }
            _level++;
            SetNextXp();
            if (OnLevelUp != null) {
                OnLevelUp();
            }
        }

        private void SetNextXp() {
            _lastLevelXp = _nextLevelXp;
            _nextLevelXp = _level * (_level + _extraAdd) * _multiplier;
        }

        public void ForceAdvanceLevel() {
            _experience.ChangeValue(_nextLevelXp);
            _level++;
            SetNextXp();
        }

        public void OverrideLevel(int level) {
            _level = level;
        }

        public int Level { get { return _level; } }
        public FloatValueHolder TotalXp { get { return _experience; } }
        public float XpNeeded { get { return _nextLevelXp; } }

        public float Percent {
            get {
                if (_experience.Value > _lastLevelXp) {
                    return (_experience.Value - _lastLevelXp) / (_nextLevelXp - _lastLevelXp);
                }

                return _experience.Value / _nextLevelXp;
            }
        }
    }
}