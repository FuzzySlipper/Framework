using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Priority(Priority.Highest)]
    [System.Serializable]
	public sealed class ModelLoaderComponent : IComponent {
        
        public bool OnlyActiveWhileEquipped;
        public string ModelName;
        public CachedGenericComponent<IModelComponent> LoadedModel;
        public List<SerializableType> LoadedComponents = new List<SerializableType>();

        public ModelLoaderComponent(bool onlyActiveWhileEquipped, string modelName) {
            OnlyActiveWhileEquipped = onlyActiveWhileEquipped;
            ModelName = modelName;
        }

        public ModelLoaderComponent(SerializationInfo info, StreamingContext context) {
            OnlyActiveWhileEquipped = info.GetValue(nameof(OnlyActiveWhileEquipped), OnlyActiveWhileEquipped);
            ModelName = info.GetValue(nameof(ModelName), ModelName);
            LoadedModel = info.GetValue(nameof(LoadedModel), LoadedModel);
            LoadedComponents = info.GetValue(nameof(LoadedComponents), LoadedComponents);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(OnlyActiveWhileEquipped), OnlyActiveWhileEquipped);
            info.AddValue(nameof(ModelName), ModelName);
            info.AddValue(nameof(LoadedModel), LoadedModel);
            info.AddValue(nameof(LoadedComponents), LoadedComponents);
        }
    }
}
