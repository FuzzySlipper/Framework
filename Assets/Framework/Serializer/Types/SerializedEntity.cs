using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class SerializedEntity : ISerializable {

        private Entity _entity;
        private List<IComponent> _allComponents = new List<IComponent>();

        public Entity Entity { get => _entity; }
        public List<IComponent> AllComponents { get => _allComponents; }

        public SerializedEntity(SerializationInfo info, StreamingContext context) {
            _allComponents = info.GetValue(nameof(_allComponents), _allComponents);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_allComponents), _allComponents);
        }

        public SerializedEntity(Entity entity) {
            var dict = EntityController.GetEntityComponentDict(entity);
            _entity = entity;
            foreach (var cref in dict) {
                _allComponents.Add((IComponent)cref.Value.Get());
            }
        }
    }
}
