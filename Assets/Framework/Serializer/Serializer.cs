using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Object = UnityEngine.Object;

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
        public static HashSet<System.Type> RestrictedTypes = new HashSet<Type>() { typeof(Transform) };
        private static Dictionary<int, PrefabEntity> _currentList = new Dictionary<int, PrefabEntity>();
        private static int _globalIndex = 0;

        public static ISerializer MainSerializer { get; set; }

        private static void CheckMainSerializer() {
            if (MainSerializer == null)
                MainSerializer = new JsonDotNetSerializer();
        }

        public static void Initialize(ISerializer serializer) {
            MainSerializer = serializer;
        }

        public static IUnitySerializable CreateSerializableObject<T>(T obj) {
            return new SerializedUnityComponent<T>(obj);
        }

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

        public static PrefabEntity GetPrefabEntity(int id) {
            return _currentList.TryGetValue(id, out var entity) ? entity : null;
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
        
        public static void DebugLogError(string error, string method, Object context = null) {
            Debug.LogError(string.Format("Serializer: {0} {1}", method, error), context);
        }

        /// <summary>
        ///     Deserialize a string with the Main Serializer
        /// </summary>
        /// <param name="str">String to deserialize.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T Deserialize<T>(string str) {
            CheckMainSerializer();
            var t = typeof(T);
            if (t.IsSerializable)
                return MainSerializer.Deserialize<T>(str);
            DebugLogError("The Given Type is not Serializeable", "Deserialize");
            return default(T);
        }

        /// <summary>
        ///     Deserialize the given string with the given serializer.
        /// </summary>
        /// <param name="str">String to deserialize.</param>
        /// <param name="serializer">The Serializer.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T Deserialize<T>(string str, ISerializer serializer) {
            CheckMainSerializer();
            var t = typeof(T);
            if (t.IsSerializable)
                return serializer.Deserialize<T>(str);
            DebugLogError("The Given Type is not Serializeable", "Deserialize");
            return default(T);
        }

        /// <summary>
        ///     Serialize an Object with the given serializer.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="serializer">The Serializer.</param>
        public static string Serialize(object obj, ISerializer serializer) {
            CheckMainSerializer();
            if (obj.GetType().IsSerializable) {
                return serializer.Serialize(obj);
            }
            DebugLogError("The Given Type is not Serializeable", "Serialize", (Object) obj);
            return null;
        }

        /// <summary>
        ///     Serialize an Object with the Main Serializer.
        /// </summary>
        /// <param name="obj">Object to Serialize.</param>
        public static string Serialize(object obj) {
            CheckMainSerializer();
            if (obj.GetType().IsSerializable)
                return MainSerializer.Serialize(obj);
            DebugLogError("The Given Type is not Serializeable", "Serialize", (Object) obj);
            return null;
        }
    }
}
