using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class SerializedEntity : ISerializable {

        private Entity _entity;

        private int[] _tags;
        private string _entityName;
        private int _entityId;
        private List<IComponent> _allComponents = new List<IComponent>();

        public Entity Entity { get => _entity; }
        public List<IComponent> AllComponents { get => _allComponents; }

        public SerializedEntity(SerializationInfo info, StreamingContext context) {
            _allComponents = info.GetValue(nameof(_allComponents), _allComponents);
            _entityName = info.GetValue(nameof(_entityName), _entityName);
            _entityId = info.GetValue(nameof(_entityId), _entityId);
            _tags = info.GetValue(nameof(_tags), _tags);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_allComponents), _allComponents);
            info.AddValue(nameof(_entityName), _entityName);
            info.AddValue(nameof(_entityId), _entityId);
            info.AddValue(nameof(_tags), _tags);
        }

        public SerializedEntity(Entity entity) {
            var components = entity.GetAllComponents();
            _entity = entity;
            _entityName = entity.Name;
            _entityId = entity.Id;
            _tags = entity.Tags.Tags;
            foreach (var cref in components) {
                _allComponents.Add((IComponent)cref.Get());
            }
        }

        public Entity Restore() {
            _entity = Entity.Restore(_entityName, _entityId);
            for (int i = 0; i < _allComponents.Count; i++) {
                _entity.Add(_allComponents[i]);
            }
            _entity.Tags.Replace(_tags);
            return _entity;
        }
    }
}
