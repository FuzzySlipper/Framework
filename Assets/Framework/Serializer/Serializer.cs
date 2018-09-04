using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PixelComrades {
    public static class Serializer {
        public static readonly JsonSerializerSettings ConverterSettings = new JsonSerializerSettings {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new JsonContractResolver(),
            TypeNameHandling = TypeNameHandling.Objects,
            Error = (sender, args) => {
                Debug.LogFormat("Sender {0}", sender != null ? sender.ToString() : "null");
                Debug.LogErrorFormat("Current {0}:{1}", args.CurrentObject != null ? args.CurrentObject.ToString() : "null", args.ErrorContext);
            }
        };
        public static readonly JsonSerializer JsonCustom = JsonSerializer.Create(ConverterSettings);
        public static HashSet<System.Type> RestrictedTypes = new HashSet<Type>() {
            typeof(Transform)
        };

        public static IUnitySerializable CreateSerializableObject<T>(T obj) {
            return new SerializedUnityComponent<T>(obj);
        }

        private static Dictionary<int, PrefabEntity> _currentList = new Dictionary<int, PrefabEntity>();
        private static int _globalIndex = 0;

        public static void ResetGlobalSerializationIndex() {
            _globalIndex = 0;
        }

        public static int GetSerializedId() {
            _globalIndex++;
            return _globalIndex;
        }

        public static void ClearList() {
            _currentList.Clear();
        }

        public static PrefabEntity GetEntity(int id) {
            PrefabEntity entity;
            return _currentList.TryGetValue(id, out entity) ? entity : null;
        }

        public static void AddEntity(PrefabEntity we) {
            if (_currentList.ContainsKey(we.Metadata.SerializationId)) {
                Debug.LogErrorFormat("Duplicate Id {0} current {1} parent {3} new {2}",
                    we.Metadata.SerializationId, we.name,
                    _currentList[we.Metadata.SerializationId].name,
                    we.transform.parent != null ? we.transform.parent.name : "None");
                _globalIndex = MathEx.Max(we.Metadata.SerializationId + 1, _globalIndex);
                we.Metadata.UpdateSerializedId(true);
            }
            _globalIndex = MathEx.Max(we.Metadata.SerializationId + 1, _globalIndex);
            _currentList.Add(we.Metadata.SerializationId, we);
        }

    }
}
