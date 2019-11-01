using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class GenericSceneObjectComponent : IComponent {

        public GenericSceneObject[] SceneObjects;

        private GameObject _root;
        
        public GenericSceneObjectComponent(GameObject root) {
            _root = root;
            Rescan();
        }

        public void Rescan() {
            SceneObjects = _root.GetComponentsInChildren<GenericSceneObject>(true);
        }

        public GameObject GetObject(string id) {
            for (int i = 0; i < SceneObjects.Length; i++) {
                if (SceneObjects[i].ID.CompareCaseInsensitive(id)) {
                    return SceneObjects[i].gameObject;
                }
            }
            return null;
        }
        
        public GenericSceneObjectComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
