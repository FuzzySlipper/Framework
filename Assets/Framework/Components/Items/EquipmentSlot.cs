using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class EquipmentSlot: IEquipmentHolder, ISerializable {

        public Action<Entity> OnItemChanged { get;set; }

        private string _targetSlot;
        private CachedTransform _equipTr;
        private CachedEntity _item = new CachedEntity();
        private CachedComponent<EquipmentSlots> _slots;
        private string _lastEquipStatus = "";
        
        public EquipmentSlot(EquipmentSlots owner, string targetSlot, string name, Transform equipTr) {
            _targetSlot = targetSlot;
            _equipTr = new CachedTransform(equipTr);
            Name = name;
            _slots = new CachedComponent<EquipmentSlots>(owner);
            CurrentStats = new List<StatModHolder>();
            CompatibleSlots = new[] {_targetSlot};
        }

        public EquipmentSlot(SerializationInfo info, StreamingContext context) {
            _targetSlot = info.GetValue(nameof(_targetSlot), _targetSlot);
            _equipTr = info.GetValue(nameof(_equipTr), _equipTr);
            _item = info.GetValue(nameof(_item), _item);
            _slots = info.GetValue(nameof(_slots), _slots);
            _lastEquipStatus = info.GetValue(nameof(_lastEquipStatus), _lastEquipStatus);
            CurrentStats = info.GetValue(nameof(CurrentStats), CurrentStats);
            CompatibleSlots = new[] {_targetSlot};
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_targetSlot), _targetSlot);
            info.AddValue(nameof(_equipTr), _equipTr);
            info.AddValue(nameof(_item), _item);
            info.AddValue(nameof(_slots), _slots);
            info.AddValue(nameof(_lastEquipStatus), _lastEquipStatus);
            info.AddValue(nameof(CurrentStats), CurrentStats);
        }

        public Entity Owner { get { return _slots.Value.Owner; } }
        public IEntityContainer Container { get { return _slots.Value; } }
        public Entity Item { get { return _item; } set { _item.Set(value);} }
        public string LastEquipStatus { get { return _lastEquipStatus; } set { _lastEquipStatus = value; } }
        public string TargetSlot { get { return _targetSlot; } }
        public string Name { get; }
        public Transform EquipTr { get => _equipTr; }
        public Type[] RequiredTypes { get => _requiredTypes; }
        public string[] CompatibleSlots { get; }
        public List<StatModHolder> CurrentStats { get; }
        
        private static System.Type[] _requiredTypes = new[] {
            typeof(Equipment),
        };
    }
}