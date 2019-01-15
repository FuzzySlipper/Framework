using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PixelComrades {

    public abstract class GenericDatabase : ScriptableObject {
        public bool SearchEntireProject = false;
        public string[] CustomSearchPaths = {"GameData\\Items\\"};
        public abstract int Count { get; }

        public abstract IEnumerable<IGenericData> GetDataGeneric();
        public abstract IEnumerable<ScriptableObject> GetDataScriptable();
        [Button("Clear Data")]
        public virtual void ClearData(){}
        [Button("Refresh Database")]
        public virtual void RefreshAssets(){}
        [Button("Check For Duplicates")]
        public virtual void CheckForDuplicates(){}
        public virtual void CreateAsset(string assetName = "", string path = ""){}
    }

    public interface IGenericData {
        string Id { get; }
    }

    public abstract class GenericDatabase<T> : GenericDatabase where T : ScriptableObject, IGenericData {

        [SerializeField] private List<T> _data = new List<T>();
        [SerializeField] private List<ScriptableObject> _scriptableChildren = new List<ScriptableObject>();

        private Dictionary<string, T> _dataLookup = new Dictionary<string, T>();
        private Dictionary<string, T> _filenameLookup = new Dictionary<string, T>();

        public override int Count { get { return _data.Count; } }

        protected Dictionary<string, T> DataLookup {
            get {
                CheckData();
                return _dataLookup;
            }
        }

        protected Dictionary<string, T> FileNameLookup {
            get {
                CheckData();
                return _filenameLookup;
            }
        }

        private List<T> AllData { get { return _data; } }

        public virtual void CheckData() {
            if (_dataLookup == null || _dataLookup.Count == 0) {
                _dataLookup = new Dictionary<string, T>();
                _filenameLookup = new Dictionary<string, T>();
                ProcessData();
            }
        }

        public void RunActionOnData(System.Action<T> del) {
            var enumerator = DataLookup.GetEnumerator();
            try {
                while (enumerator.MoveNext()) {
                    var cell = enumerator.Current.Value;
                    del(cell);
                }
            }
            finally {
                enumerator.Dispose();
            }
        }

        protected virtual void ProcessData() {
            _dataLookup.Clear();
            _filenameLookup.Clear();
            for (int i = 0; i < _scriptableChildren.Count; i++) {
                var obj = _scriptableChildren[i] as T;
                if (obj != null) {
                    var key = obj.Id;
                    if (!_dataLookup.TryAdd(obj.Id, obj)) {
                        Debug.LogFormat("couldn't add {0} {1} already contains {2} {3}", key, obj.name, _dataLookup[key].Id, _dataLookup[key].name);
                    }
                    if (!_filenameLookup.TryAdd(obj.Id, obj)) {
                        Debug.LogFormat("file couldn't add {0} {1} already contains {2} {3}", key, obj.name, _filenameLookup[key].Id, _filenameLookup[key].name);
                    }
                }
            }
            for (var i = 0; i < _data.Count; i++) {
                var obj = _data[i];
                var key = obj.Id;
                if (!_dataLookup.TryAdd(key, obj)) {
                    Debug.LogFormat("couldn't add {0} {1} already contains {2} {3}",key, obj.name, _dataLookup[key].Id, _dataLookup[key].name);
                }
                key = obj.name;
                if (!_filenameLookup.TryAdd(key, obj)) {
                    Debug.LogFormat("file couldn't add {0} {1} already contains {2} {3}", key, obj.name, _filenameLookup[key].Id, _filenameLookup[key].name);
                }
            }
        }

        public bool ContainsData(string dataName) {
            if (!DataLookup.ContainsKey(dataName.ToLower())) {
                return FileNameLookup.ContainsKey(dataName);
            }
            return true;
        }

        public TV GetData<TV>(string dataName) where TV : class, T {
            T d;
            if (!DataLookup.TryGetValue(dataName.ToLower(), out d)) {
                return FileNameLookup.TryGetValue(dataName, out d) ? (TV) d : null;
            }
            return (TV) d;
        }

        public T GetData(string dataName) {
            T d;
            if (!DataLookup.TryGetValue(dataName.ToLower(), out d)) {
                return FileNameLookup.TryGetValue(dataName, out d) ? d : null;
            }
            return d;
        }


        public override void CheckForDuplicates() {
            _dataLookup.Clear();
            _filenameLookup.Clear();
            for (var i = 0; i < _data.Count; i++) {
                if (_dataLookup.ContainsKey(_data[i].Id)) {
                    Debug.LogErrorFormat("Key {0} already in database", _data[i].Id);
                }
                else {
                    _dataLookup.Add(_data[i].Id, _data[i]);
                }
                if (_filenameLookup.ContainsKey(_data[i].name)) {
                    Debug.LogErrorFormat("Key {0} already in database", _data[i].name);
                }
                else {
                    _filenameLookup.Add(_data[i].name, _data[i]);
                }
            }
        }

        public override IEnumerable<IGenericData> GetDataGeneric() {
            return AllData.Cast<IGenericData>();
        }

        public override IEnumerable<ScriptableObject> GetDataScriptable() {
            return AllData.Cast<ScriptableObject>();
        }


        public override void ClearData() {
            AllData.Clear();
            if (_dataLookup != null) {
                _dataLookup.Clear();
                _filenameLookup.Clear();
            }
        }

        protected virtual bool CanAddData(T template) {
            return true;
        }

#if UNITY_EDITOR

        public override void RefreshAssets() {
            AllData.Clear();
            var sDataPath = Application.dataPath;
            if (SearchEntireProject) {
                AddDataInPath(sDataPath);
            }
            else {
                foreach (var datablockSearchPath in CustomSearchPaths) {
                    var sFolderPath = sDataPath.Substring(0, sDataPath.Length - 6) + "Assets\\" + datablockSearchPath;
                    AddDataInPath(sFolderPath);
                }
            }
            ProcessData();
        }

        private void AddDataInPath(string sFolderPath) {
            var dirName = Directory.GetDirectories(sFolderPath);
            var directories = new List<string>();
            directories.AddRange(dirName);
            foreach (var directory in directories) {
                AddDataInPath(directory);
            }
            CheckAtPath(sFolderPath, "*.asset");
            CheckAtPath(sFolderPath, "*.prefab");
            EditorUtility.SetDirty(this);
        }

        private void CheckAtPath(string folderPath, string extension) {
            var sDataPath = Application.dataPath;
            // get the system file paths of all the files in the asset folder
            var aFilePaths = Directory.GetFiles(folderPath, extension);
            // enumerate through the list of files loading the assets they represent and getting their type		
            foreach (var sFilePath in aFilePaths) {
                var sAssetPath = sFilePath.Substring(sDataPath.Length - 6);

                var data = AssetDatabase.LoadAssetAtPath(sAssetPath, typeof(T)) as T;
                if (data == null || !CanAddData(data)) {
                    continue;
                }
                AllData.Add(data);
            }
        }

        protected void AddDataInPathGeneric<TV>(string folderPath, string extension, List<TV> list) where TV : UnityEngine.Object {
            var dirName = Directory.GetDirectories(folderPath);
            var directories = new List<string>();
            directories.AddRange(dirName);
            foreach (var directory in directories) {
                AddDataInPath(directory);
            }
            CheckAtPathGeneric<TV>(folderPath, extension, list);
            EditorUtility.SetDirty(this);
        }

        protected void CheckAtPathGeneric<TV>(string folderPath, string extension, List<TV> list) where TV : UnityEngine.Object {
            var sDataPath = Application.dataPath;
            // get the system file paths of all the files in the asset folder
            var aFilePaths = Directory.GetFiles(folderPath, extension);
            // enumerate through the list of files loading the assets they represent and getting their type		
            foreach (var sFilePath in aFilePaths) {
                var sAssetPath = sFilePath.Substring(sDataPath.Length - 6);

                var data = AssetDatabase.LoadAssetAtPath(sAssetPath, typeof(TV)) as TV;
                if (data == null) {
                    continue;
                }
                list.Add(data);
            }
        }
        
        public override void CreateAsset(string assetName = "", string path = "") {
            var asset = CreateInstance<T>();
            var assetPathAndName = "";
            if (assetName == "") {
                if (path == "") {
                    path = AssetDatabase.GetAssetPath(Selection.activeObject);
                    if (path == "") {
                        path = "Assets";
                    }
                    else if (Path.GetExtension(path) != "") {
                        path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
                    }
                }
                assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T) + ".asset");
            }
            else {
                assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + assetName + ".asset");
            }
            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
        }

        public TV CreateAsset<TV>(string assetName, string path) where TV : T {
            var asset = CreateInstance<TV>();
            var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/" + path + assetName + ".asset");
            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }

        public string GetUniqueName(string dataName) {
            ProcessData();
            if (!_filenameLookup.ContainsKey(dataName) && !_dataLookup.ContainsKey(dataName)) {
                return dataName;
            }
            for (var x = 1; x < 1000; x++) {
                var newDatablockname = dataName + " " + x;
                if (!_filenameLookup.ContainsKey(newDatablockname) && !_dataLookup.ContainsKey(newDatablockname)) {
                    return newDatablockname;
                }
            }
            Debug.LogError("Unable to find a unique name for " + dataName);
            return new System.Guid().ToString();
        }

        public TV CreateChildAsset<TV>(string assetName) where TV : T {
            var targetAsset = ScriptableObject.CreateInstance<TV>();
            targetAsset.name = assetName;
            AssetDatabase.AddObjectToAsset(targetAsset, AssetDatabase.GetAssetPath(this));
            _scriptableChildren.Add(targetAsset);
            return targetAsset;
        }

        public void DeleteChildAsset(ScriptableObject asset) {
            _scriptableChildren.Remove(asset);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
            
        }
#endif
    }
}
