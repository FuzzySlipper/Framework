using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class AddModImpact : IActionImpact, ISerializable {

        private float _length;
        private string _targetStat;
        private float _normalizedPercent;
        private string _iconLocation;
        private Sprite _icon;
        private string _id;
        private CachedStat<BaseStat> _powerStat;

        private static FastString _fastString = new FastString();
        public float Power { get { return _powerStat.Value * _normalizedPercent; } }

        public AddModImpact(float length, string targetStat, float normalizedPercent, BaseStat powerStat, IconComponent icon) {
            _length = length;
            _targetStat = targetStat;
            _normalizedPercent = normalizedPercent;
            _powerStat = new CachedStat<BaseStat>(powerStat);
            _icon = icon;
            _iconLocation = icon?.IconLocation;
            _id = System.Guid.NewGuid().ToString();
        }

        public AddModImpact(SerializationInfo info, StreamingContext context) {
            _length = info.GetValue(nameof(_length), _length);
            _targetStat = info.GetValue(nameof(_targetStat), _targetStat);
            _normalizedPercent = info.GetValue(nameof(_normalizedPercent), _normalizedPercent);
            _id = info.GetValue(nameof(_id), _id);
            _powerStat = info.GetValue(nameof(_powerStat), _powerStat);
            _iconLocation = info.GetValue(nameof(_iconLocation), _iconLocation);
            _icon = ItemPool.LoadAsset<Sprite>(_iconLocation);
        }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_length), _length);
            info.AddValue(nameof(_targetStat), _targetStat);
            info.AddValue(nameof(_normalizedPercent), _normalizedPercent);
            info.AddValue(nameof(_id), _id);
            info.AddValue(nameof(_powerStat), _powerStat);
            info.AddValue(nameof(_iconLocation), _iconLocation);
        }

        public void ProcessImpact(CollisionEvent collisionEvent, ActionStateEvent stateEvent) {
            if (collisionEvent.Hit <= 0) {
                return;
            }
            var stat = stateEvent.Target.Stats.Get(_targetStat);
            if (stat == null) {
                return;
            }
            if (stat.HasMod(_id)) {
                World.Get<ModifierSystem>().RemoveStatMod(_id);
            }
            stat.AddValueMod(new BaseStat.StatValueMod(Power, _id));
            _fastString.Clear();
            _fastString.Append("+");
            _fastString.Append(Power);
            _fastString.Append(" ");
            _fastString.Append(stat.Label);
            var label = _fastString.ToString();
            World.Get<ModifierSystem>().AddStatRemovalTimer(new RemoveStatModifier(stat, new ModEntry(label, label, _id, _length, 
            collisionEvent.Origin.Entity, stateEvent.Target.Entity, _icon)));
            collisionEvent.Target.Post(new ModifiersChanged(stateEvent.Target.Entity));
        }
    }
}