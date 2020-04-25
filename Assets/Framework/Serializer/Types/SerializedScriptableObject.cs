using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class SerializedDatabaseCollection : ISerializable {
        public SerializedScriptableObjectCollection[] Databases;

        public SerializedDatabaseCollection(IEnumerable<ScriptableDatabase> list) {
            Databases = new SerializedScriptableObjectCollection[list.Count()];
            int index = 0;
            foreach (var child in list) {
                Databases[index] = new SerializedScriptableObjectCollection(child, child.AllObjects);
                index++;
            }
        }

        public SerializedDatabaseCollection() {
        }

        public SerializedDatabaseCollection(SerializationInfo info, StreamingContext context) {
            Databases = info.GetValue(nameof(Databases), Databases);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Databases), Databases);
        }

        public void Restore() {
            Create();
            Connect();
        }

        public void Create() {
            if (Databases == null) {
                return;
            }
            for (int i = 0; i < Databases.Length; i++) {
                Databases[i].Create();
            }
        }

        public void Connect() {
            if (Databases == null) {
                return;
            }
            for (int i = 0; i < Databases.Length; i++) {
                Databases[i].Connect();
            }
        }
    }
    
    public class SerializedScriptableObjectCollection : ISerializable {
        public SerializedScriptableObject Main;
        public SerializedScriptableObject[] Children;

        public SerializedScriptableObjectCollection(ScriptableObject main, IEnumerable<UnityEngine.Object> list) {
            Main = new SerializedScriptableObject(main);
            Children = new SerializedScriptableObject[list.Count()];
            int index = 0;
            foreach (var child in list) {
                Children[index] = new SerializedScriptableObject(child as ScriptableObject);
                index++;
            }
        }

        public SerializedScriptableObjectCollection() {
        }

        public SerializedScriptableObjectCollection(SerializationInfo info, StreamingContext context) {
            Children = info.GetValue(nameof(Children), Children);
            Main = info.GetValue(nameof(Main), Main);
            
        }

        public void Restore() {
            Create();
            Connect();
        }

        public void Create() {
            Main.Create();
            if (Children == null) {
                return;
            }
            for (int i = 0; i < Children.Length; i++) {
                Children[i].Create();
            }
        }

        public void Connect() {
            Main.Connect();
            if (Children == null) {
                return;
            }
            for (int i = 0; i < Children.Length; i++) {
                Children[i].Connect();
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Main), Main);
            info.AddValue(nameof(Children), Children);
        } 
    }

    public class SerializedScriptableObject : ISerializable {

        [NonSerialized]public ScriptableObject Value;
        public System.Type Type;
        public string Path;
        public string Guid;
        public string SerializedValue;

        public SerializedScriptableObject(ScriptableObject value) {
            Value = value;
            Type = value.GetType();
#if UNITY_EDITOR
            Path = UnityEditor.AssetDatabase.GetAssetPath(value);
            Guid = UnityEditor.AssetDatabase.AssetPathToGUID(Path);
            SerializedValue = UnityEditor.EditorJsonUtility.ToJson(Value, true);
#endif
        }

        public void Restore() {
            Create();
            Connect();
        }

        public void Create() {
#if UNITY_EDITOR
            var value = UnityEditor.AssetDatabase.LoadMainAssetAtPath(Path);
            if (value == null) {
                value = UnityEditor.AssetDatabase.LoadMainAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(Guid));
            }
            if (value == null) {
                value = ScriptableObject.CreateInstance(Type);
                UnityEditor.AssetDatabase.CreateAsset(value, Path);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }
            Value = value as ScriptableObject;
#endif           
        }

        public void Connect() {
#if UNITY_EDITOR
            //UnityEditor.EditorUtility.CopySerialized();
            UnityEditor.EditorJsonUtility.FromJsonOverwrite(SerializedValue, Value);
            UnityEditor.EditorUtility.SetDirty(Value);
#endif            
        }

        public SerializedScriptableObject() { }

        public SerializedScriptableObject(SerializationInfo info, StreamingContext context) {
            Path = info.GetValue(nameof(Path), Path);
            Guid = info.GetValue(nameof(Guid), "");
            SerializedValue = info.GetValue(nameof(SerializedValue), "");
            Type = ParseUtilities.ParseType(info.GetValue(nameof(Type), ""));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Path), Path);
            info.AddValue(nameof(Type), Type.ToString());
            info.AddValue(nameof(Guid), Guid);
            info.AddValue(nameof(SerializedValue), SerializedValue);
        }
    }
}
