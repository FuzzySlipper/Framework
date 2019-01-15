using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class SerializedScene : ISerializable {

        [SerializeField] private List<SerializedGameObject> _objects;
        [SerializeField] private string _name;

        public string Name { get { return _name; } }
        public List<SerializedGameObject> Objects { get { return _objects; } }

        public SerializedScene() { }

        public SerializedScene(SerializationInfo info, StreamingContext context) {
            _name = (string)info.GetValue("Name", typeof(string));
            _objects = (List<SerializedGameObject>)info.GetValue("Objects", typeof(List<SerializedGameObject>));
        }

        public SerializedScene(GameObject root, string name) {
            _name = name;
            _objects = new List<SerializedGameObject>();
            Serializer.ResetGlobalSerializationIndex();
            ResetSerializedIds(root.transform);
            ScanChildren(root);
        }

        public SerializedScene(GameObject[] roots, string name) {
            _name = name;
            _objects = new List<SerializedGameObject>();
            Serializer.ResetGlobalSerializationIndex();
            for (int i = 0; i < roots.Length; i++) {
                ResetSerializedIds(roots[i].transform);
            }
            for (int i = 0; i < roots.Length; i++) {
                ScanChildren(roots[i]);
            }
        }

        private void ResetSerializedIds(Transform root) {
            var we = root.GetComponentsInChildren<PrefabEntity>();
            for (int i = 0; i < we.Length; i++) {
                we[i].Metadata.UpdateSerializedId(true);
            }
        }

        private void ScanChildren(GameObject root) {
            foreach (Transform child in root.transform) {
                if (child.gameObject.hideFlags == HideFlags.DontSave || child.gameObject.hideFlags == HideFlags.HideAndDontSave) {
                    continue;
                }
                _objects.Add(new SerializedGameObject(child.gameObject, child.GetComponent<PrefabEntity>()));
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Name", _name, typeof(string));
            info.AddValue("Objects", _objects, typeof(List<SerializedGameObject>));
        }

        public GameObject Restore(GameObject root) {
            if (root == null) {
                root = new GameObject();
            }
            root.name = _name;
            for (var i = 0; i < _objects.Count; i++) {
                var child = _objects[i].ToGameObject(root.transform);
                child.transform.SetParent(root.transform);
            }
            Serializer.ClearList();
            var worldEntities = root.GetComponentsInChildren<PrefabEntity>();
            RegisterSerializedIds(worldEntities, true);
            RegisterSerializedIds(worldEntities, false);
            for (int i = 0; i < worldEntities.Length; i++) {
                worldEntities[i].Metadata.ComponentDiff.ApplyDifferences(worldEntities[i]);
            }
            return root;
        }

        public IEnumerator Restore(GameObject root, float pause) {
            if (root == null) {
                root = new GameObject();
            }
            root.name = _name;
            for (var i = 0; i < _objects.Count; i++) {
                var child = _objects[i].ToGameObject(root.transform);
                child.transform.SetParent(root.transform);
                yield return pause;
            }
            Serializer.ClearList();
            var worldEntities = root.GetComponentsInChildren<PrefabEntity>();
            RegisterSerializedIds(worldEntities, true);
            RegisterSerializedIds(worldEntities, false);
            for (int i = 0; i < worldEntities.Length; i++) {
                worldEntities[i].Metadata.ComponentDiff.ApplyDifferences(worldEntities[i]);
            }
        }

        private void RegisterSerializedIds(PrefabEntity[] prefabEntities, bool onlyValid) {
            for (int i = 0; i < prefabEntities.Length; i++) {
                var we = prefabEntities[i];
                if (onlyValid && !we.Metadata.HasValidSerializedId) {
                    continue;
                }
                if (!onlyValid && we.Metadata.HasValidSerializedId) {
                    continue;
                }
                Serializer.AddEntity(we);
            }
        }
    }
}
