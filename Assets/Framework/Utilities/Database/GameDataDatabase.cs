using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector;
#endif

namespace PixelComrades {
    public class GameDataDatabase<T> : ScriptableObject where T : UnityEngine.Object {

        [SerializeField] private List<T> _data = new List<T>();

        private Dictionary<string, T> _dataLookup = new Dictionary<string, T>();
        private Dictionary<string, T> _filenameLookup = new Dictionary<string, T>();

        public string[] CustomSearchPaths = {
            "GameData\\Items\\"
        };

        public bool SearchEntireProject = true;

        public int Count { get { return _data.Count; } }

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

        public List<T> AllData { get { return _data; } }

        protected void ProcessData() {
            for (var i = 0; i < _data.Count; i++) {
                _dataLookup.Add(_data[i].name.ToLower(), _data[i]);
                _filenameLookup.Add(_data[i].name, _data[i]);
            }
        }

        public void CheckForDuplicate() {
            _dataLookup.Clear();
            _filenameLookup.Clear();
            for (var i = 0; i < _data.Count; i++) {
                if (_dataLookup.ContainsKey(_data[i].name.ToLower())) {
                    Debug.LogErrorFormat("Key {0} already in database", _data[i].name.ToLower());
                }
                else {
                    _dataLookup.Add(_data[i].name.ToLower(), _data[i]);
                }
                if (_filenameLookup.ContainsKey(_data[i].name)) {
                    Debug.LogErrorFormat("Key {0} already in database", _data[i].name);
                }
                else {
                    _filenameLookup.Add(_data[i].name, _data[i]);
                }
                
                
            }
        }

        protected virtual bool CanAddData(T template) {
            var go = template as Component;
            if (go != null && go.gameObject.CompareTag(StringConst.TagDummy)) {
                return false;
            }
            return true;
        }

        protected IEnumerable<V> GetData<V>() where V : T {
            return AllData.Where(d => d is V).Cast<V>();
        }

        public virtual void CheckData() {
            if (_dataLookup == null || _dataLookup.Count == 0) {
                _dataLookup = new Dictionary<string, T>();
                _filenameLookup = new Dictionary<string, T>();
                ProcessData();
            }
        }

        public void ClearTemplates() {
            AllData.Clear();
            if (_dataLookup != null) {
                _dataLookup.Clear();
                _filenameLookup.Clear();
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

        public IEnumerable<Entity> GetDataGeneric() {
            return AllData.Cast<Entity>();
        }

        public IEnumerable<T> GetData() {
            return AllData;
        }

        //public string GetUniqueName(string dataName) {
        //    var existing = GetDataGeneric().Any(d => d.name == dataName);
        //    if (!existing) {
        //        return dataName;
        //    }

        //    for (var x = 1; x < 1000; x++) {
        //        var newDatablockname = dataName + " " + x;
        //        existing = GetDataGeneric().Any(d => d.name == newDatablockname);
        //        if (!existing) {
        //            return newDatablockname;
        //        }
        //    }
        //    Debug.LogError("Unable to find a unique name for " + dataName);
        //    return dataName;
        //}

#if UNITY_EDITOR
        [Button("Refresh Database")]
        public virtual void RefreshAssets() {
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
        }

        /// <summary>
        ///     Add the datablocks in a path
        /// </summary>
        /// <param name="sFolderPath">Path</param>
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
#endif
    }
}