using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace PixelComrades {
    public class ItemPool : ScriptableSingleton<ItemPool> {

        //private Transform _activeSceneTr = null;
        private Transform _inactiveSceneTr = null;

        private Dictionary<int, PrefabEntity> _referenceItems = new Dictionary<int, PrefabEntity>();
        private Dictionary<string, int> _stringToId = new Dictionary<string, int>();
        private Dictionary<int, Queue<PrefabEntity>> _pooledDict = new Dictionary<int, Queue<PrefabEntity>>();

        private static List<PrefabEntity> _sceneObjects = new List<PrefabEntity>();
        public static List<PrefabEntity> SceneObjects { get => _sceneObjects; }
        private static Transform PoolTransform {
            get {
                if (Main._inactiveSceneTr == null && !TimeManager.IsQuitting) {
                    Main._inactiveSceneTr = Game.GetMainChild("InactivePool");
                }
                return Main._inactiveSceneTr;
            }
        }
        //public static Transform ActiveSceneTr {
        //    get {
        //        if (Main._activeSceneTr == null && !TimeManager.IsQuitting) {
        //            Main._activeSceneTr = Game.GetMainChild("SceneObjects");
        //            MessageKit.addObserver(Messages.LevelClear, DespawnScene);
        //        }
        //        return Main._activeSceneTr;
        //    }
        //}

        public static void RegisterExistingItems() {
            for (int s = 0; s < SceneManager.sceneCount; s++) {
                var scene = SceneManager.GetSceneAt(s);
                RegisterSceneEntities(scene);
            }
            SystemManager.CheckForDuplicates();
        }

        public static Queue<PrefabEntity> GetQueue(int id) {
            return Main._pooledDict.TryGetValue(id, out var queue) ? queue : null;
        }

        public static void RegisterSceneEntities(Scene scene) {
            var rootList = scene.GetRootGameObjects();
            for (int l = 0; l < rootList.Length; l++) {
                var worldEntities = rootList[l].GetComponentsInChildren<PrefabEntity>(true);
                HashSet<GameObject> goList = new HashSet<GameObject>();
                for (int i = 0; i < worldEntities.Length; i++) {
                    worldEntities[i].SetStatic();
                    goList.Add(worldEntities[i].gameObject);
                }
                LookForNeededEntities(rootList[l].GetComponentsInChildren<IOnCreate>(true), ref goList);
                LookForNeededEntities(rootList[l].GetComponentsInChildren<IPoolEvents>(true), ref goList);
                LookForNeededEntities(rootList[l].GetComponentsInChildren<ISystemUpdate>(true), ref goList);
                LookForNeededEntities(rootList[l].GetComponentsInChildren<ITurnUpdate>(true), ref goList);
            }
        }

        private static void LookForNeededEntities<T>(IList<T> list, ref HashSet<GameObject> goList) {
            for (int i = 0; i < list.Count; i++) {
                var c = list[i];
                var unityComponent = c as UnityEngine.Component;
                if (unityComponent == null || !NeedsComponentAdded(unityComponent, ref goList)) {
                    continue;
                }
                unityComponent.gameObject.AddComponent<PrefabEntity>().SetStatic();
                goList.Add(unityComponent.gameObject);
            }
        }

        private static bool NeedsComponentAdded(UnityEngine.Component unityComponent, ref HashSet<GameObject> goList) {
            if (unityComponent == null || unityComponent.GetComponent<PrefabEntity>() != null) {
                return false;
            }
            if (goList.Contains(unityComponent.gameObject)) {
                return false;
            }
            if (unityComponent.gameObject.GetComponent<OptOutEntityRegistration>() != null) {
                return false;
            }
            var parent = unityComponent.transform.parent;
            while (parent != null) {
                if (goList.Contains(parent.gameObject)) {
                    return false;
                }
                parent = parent.parent;
            }
            return true;
        }

        public static void ClearItemPool() {
            foreach (var pools in Main._pooledDict) {
                var list = pools.Value;
                while (list.Count > 0) {
                    var copy = list.Dequeue();
                    if (copy != null) {
                        UnityEngine.Object.Destroy(copy.gameObject);
                    }
                }
                list.Clear();
            }
        }

        public static void ClearReferences() {
            Main._referenceItems.Clear();
            Main._stringToId.Clear();
            Resources.UnloadUnusedAssets();
        }

        private static FastString _stringBuilder = new FastString();

        public static T LoadAsset<T>(string dir, string file, System.Action<T> del = null) where T : UnityEngine.Object {
            if (string.IsNullOrEmpty(file)) {
                if (!string.IsNullOrEmpty(dir)) {
                    Debug.LogFormat("Attempted to load empty file at {0}", dir);
                }
                return default(T);
            }
            _stringBuilder.Clear();
#if UNITY_EDITOR
            //var path = string.Format("{0}{1}.{2}", UnityDirs.EditorFolder, location, AssetTypeExtensions.GetExtensionFromType<T>());
            //return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            if (!Application.isPlaying) {
                _stringBuilder.Append(UnityDirs.EditorFolder);
                _stringBuilder.Append(dir);
                _stringBuilder.Append(file);
                _stringBuilder.Append(AssetTypeExtensions.GetExtensionFromType<T>());
                return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(_stringBuilder.ToString());
            }
#endif
            _stringBuilder.Append(dir);
            _stringBuilder.Append(file);
            _stringBuilder.Append(AssetTypeExtensions.GetExtensionFromType<T>());
            return LoadAsset<T>(_stringBuilder.ToString(), del);
        }

        public static string GetCombinedLocator(string dir, string file, string extension) {
            _stringBuilder.Clear();
            _stringBuilder.Append(dir);
            _stringBuilder.Append(file);
            _stringBuilder.Append(extension);
            return _stringBuilder.ToString();
        }

        private static Dictionary<int, string> _loadedAssets = new Dictionary<int, string>();
        
        public static T LoadAsset<T>(string fullFilePath, Action<T> del = null) where T : UnityEngine.Object {
            //return Resources.Load<T>(fullFilePath);
            AsyncOperationHandle<T> op = Addressables.LoadAssetAsync<T>(fullFilePath);
            if (op.IsDone) {
                if (op.OperationException != null) {
                    return null;
                }
                _loadedAssets.AddOrUpdate(op.Result.GetInstanceID(), fullFilePath);
                del?.Invoke(op.Result);
                return op.Result;
            }
            TimeManager.StartUnscaled(TaskWait<T>(op, fullFilePath, del));
            return null;
        }

        private static IEnumerator TaskWait<T>(AsyncOperationHandle<T> op, string fullFilePath, Action<T> del) where T : UnityEngine.Object {
            while (!op.IsDone) {
                yield return null;
            }
            if (op.OperationException != null) {
                yield break;
            }
            _loadedAssets.AddOrUpdate(op.Result.GetInstanceID(), fullFilePath);
            del?.Invoke(op.Result);
        }

        public static string GetAssetLocation<T>(T target) where T : UnityEngine.Object {
            return _loadedAssets.TryGetValue(target.GetInstanceID(), out var ar) ? ar : "";
        }
        
        public static PrefabEntity Spawn(string itemName, bool isSceneObject = false, bool isCulled = true) {
            return Main.SpawnByString(itemName, isSceneObject, Vector3.zero, Quaternion.identity, null, isCulled);
        }

        public static PrefabEntity Spawn(string dir, string file, Vector3 position, Quaternion rotation, bool isSceneObject = false, bool isCulled = true) {
            _stringBuilder.Clear();
            _stringBuilder.Append(dir);
            _stringBuilder.Append(file);
            return Main.SpawnByString(_stringBuilder.ToString(), isSceneObject, position, rotation, null, isCulled);
        }

        public static PrefabEntity Spawn(string itemName, Vector3 position, Quaternion rotation, bool isSceneObject = false, bool isCulled = true) {
            return Main.SpawnByString(itemName, isSceneObject, position, rotation, null, isCulled);
        }

        public static T Spawn<T>(string dir, string file, Vector3 position, Quaternion rotation, bool isSceneObject = false, bool isCulled = true) where T : MonoBehaviour {
            _stringBuilder.Clear();
            _stringBuilder.Append(dir);
            _stringBuilder.Append(file);
            return GetWorldEntityComponent<T>(Main.SpawnByString(_stringBuilder.ToString(), isSceneObject, position, rotation, null, isCulled));
        }

        public static T Spawn<T>(string itemName, bool isSceneObject = true, bool isCulled = true) where T : MonoBehaviour {
            return GetWorldEntityComponent<T>(Main.SpawnByString(itemName, isSceneObject, Vector3.zero, Quaternion.identity, null, isCulled));
        }

        public static T Spawn<T>(string itemName, Vector3 position, Quaternion rotation, bool isScene = true, bool isCulled = true) where T : MonoBehaviour {
            return GetWorldEntityComponent<T>(Main.SpawnByString(itemName, isScene, position, rotation, null, isCulled));
        }

        public static PrefabEntity Spawn(PrefabEntity targetObject, bool isSceneObject = false, bool isCulled = false) {
            return Main.GetPrefabCopy(targetObject, isSceneObject, isCulled, Vector3.zero, Quaternion.identity);
        }

        public static PrefabEntity Spawn(PrefabEntity targetObject, Vector3 pos, Quaternion rot, bool isScene = true, bool isCulled = 
        true) {
            return Main.GetPrefabCopy(targetObject, isScene, isCulled, pos, rot);
        }

        public static PrefabEntity Spawn(GameObject targetObject, Vector3 pos, Quaternion rot, bool isScene = true, bool isCulled = true) {
            return Main.GetPrefabCopy(targetObject.GetOrAddComponent<PrefabEntity>(), isScene, isCulled, pos, rot);
        }

        public static GameObject SpawnScenePrefab(GameObject targetObject, Vector3 pos, Quaternion rot) {
            return Main.GetPrefabCopy(targetObject.GetOrAddComponent<PrefabEntity>(), true, true, pos, rot).gameObject;
        }

        public static PrefabEntity Spawn(GameObject targetObject, bool isSceneObject = true, bool isCulled = true) {
            return Main.GetPrefabCopy(targetObject.GetOrAddComponent<PrefabEntity>(), isSceneObject, isCulled, Vector3.zero, Quaternion.identity);
        }

        public static T Spawn<T>(GameObject targetObject, Vector3 pos, Quaternion rot, bool isSceneObject, bool isCulled) where T : MonoBehaviour {
            return GetWorldEntityComponent<T>(Main.GetPrefabCopy(targetObject.GetOrAddComponent<PrefabEntity>(), isSceneObject, isCulled, pos, rot));
        }

        public static T Spawn<T>(PrefabEntity targetObject, Vector3 pos, Quaternion rot, bool isSceneObject = true, bool isCulled = true) where T : MonoBehaviour {
            return GetWorldEntityComponent<T>(Main.GetPrefabCopy(targetObject, isSceneObject, isCulled, pos, rot));
        }

        public static T SpawnUIPrefab<T>(GameObject targetObject, Vector3 pos, Quaternion rot) where T : MonoBehaviour {
            return GetWorldEntityComponent<T>(Main.GetPrefabCopy(targetObject.GetOrAddComponent<PrefabEntity>(), false, false, pos, rot));
        }

        public static GameObject SpawnUIPrefab(PrefabEntity targetObject, Transform parent) {
            var newObject = Main.GetPrefabCopy(targetObject, false, false, parent.position, parent.rotation, parent);
            newObject.transform.localScale = Vector3.one;
            return newObject.gameObject;
        }

        public static GameObject SpawnUIPrefab(string prefab, Transform parent) {
            var newObject = Main.SpawnByString(prefab, false, parent.transform.position, parent.transform.rotation, parent, false);
            newObject.transform.localScale = Vector3.one;
            return newObject.gameObject;
        }

        public static T SpawnUIPrefab<T>(GameObject targetObject, Transform parent) where T : MonoBehaviour {
            var newObject = Main.GetPrefabCopy(targetObject.GetOrAddComponent<PrefabEntity>(), false, false, parent.position, parent.rotation, parent);
            newObject.transform.localScale = Vector3.one;
            return GetWorldEntityComponent<T>(newObject);
        }

        public static T SpawnUIPrefab<T>(string prefab, Transform parent) where T : MonoBehaviour {
            var newObject = Main.SpawnByString(prefab, false, parent.transform.position, parent.transform.rotation, parent, false);
            newObject.transform.localScale = Vector3.one;
            return GetWorldEntityComponent<T>(newObject);
        }

        public static T SpawnUIPrefab<T>(string dir, string file, Transform parent) where T : MonoBehaviour {
            _stringBuilder.Clear();
            _stringBuilder.Append(dir);
            _stringBuilder.Append(file);
            var newObject = Main.SpawnByString(_stringBuilder.ToString(), false, parent.transform.position, parent.transform.rotation, parent, 
            false);
            newObject.transform.localScale = Vector3.one;
            return GetWorldEntityComponent<T>(newObject);
        }

        public static void Despawn(GameObject targetGameObject) {
            Main.ReturnToPool(targetGameObject);
        }

        public static void Despawn(PrefabEntity entity) {
            if (entity == null || TimeManager.IsQuitting) {
                return;
            }
            if (!Application.isPlaying && !TimeManager.IsQuitting) {
                DestroyImmediate(entity.gameObject);
                return;
            }
            Main.ReturnToPool(entity);
        }

        public static void DespawnScene() {
            if (_sceneObjects == null) {
                return;
            }
            var existing = new List<PrefabEntity>();
            var list = new List<PrefabEntity>();
            list.AddRange(_sceneObjects);
            for (int i = 0; i < list.Count; i++) {
                if (list[i] == null) {
                    continue;
                }
                if (!list[i].IsSceneObject) {
                    existing.Add(list[i]);
                    continue;
                }
                Main.ReturnToPool(list[i]);
            }
            list.Clear();
            _sceneObjects.Clear();
            _sceneObjects.AddRange(existing);
        }

        private static T GetWorldEntityComponent<T>(PrefabEntity item) where T : MonoBehaviour {
            if (item == null) {
                return null;
            }
            ;
            T returnType = item.GetComponent<T>();
            if (returnType == null) {
                returnType = item.gameObject.AddComponent<T>();
                item.Setup();
            }
            return returnType;
        }

        public static PrefabEntity GetReferencePrefab(string itemName) {
            if (!Main._stringToId.TryGetValue(itemName, out var itemId)) {
                itemId = Main.AddNewLoadedResource(itemName);
            }
            return Main._referenceItems.TryGetValue(itemId, out var prefab) ? prefab : null;
        }

        public static PrefabEntity GetReferencePrefab(int itemId) {
            return Main._referenceItems.TryGetValue(itemId, out var prefab) ? prefab : null;
        }

        private PrefabEntity SpawnByString(string itemName, bool isScene, Vector3 pos, Quaternion rot, Transform parent, bool isCulled) {
            if (string.IsNullOrEmpty(itemName)) {
                Debug.LogError(itemName + " is blank");
                return null;
            }
            if (!Application.isPlaying) {
                var go = Addressables.InstantiateAsync(itemName + ".prefab").Result;
                return go.GetOrAddComponent<PrefabEntity>();
            }
            if (!_stringToId.TryGetValue(itemName, out var itemId)) {
                itemId = AddNewLoadedResource(itemName);
            }
            PrefabEntity item = GetPooledEntity(itemId);
            if (item == null) {
                item = GetNewStringLoaded(itemId);
            }
            if (item == null) {
                return null;
            }
            SetEntityPosition(item, isScene, pos, rot, parent);
            item.Register(isScene, isCulled);
            item.SetActive(true);
            return item;
        }

        private PrefabEntity GetPrefabCopy(PrefabEntity prefab, bool isScene, bool isCulled, Vector3 pos, Quaternion rot, Transform parent = null) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                var go = (GameObject) UnityEditor.PrefabUtility.InstantiatePrefab(prefab.gameObject);
                go.transform.position = pos;
                go.transform.rotation = rot;
                return go.GetOrAddComponent<PrefabEntity>();
            }
#endif
            if (prefab == null) {
                Debug.LogError("spawn is null");
                return null;
            }
            if (prefab.PrefabId == 0) {
                prefab.SetId(GetResourcePath(prefab.gameObject));
            }
            PrefabEntity newItem = GetPooledEntity(prefab.PrefabId);
            if (newItem == null) {
                newItem = CreateNewPrefab(prefab);
            }
            SetEntityPosition(newItem, isScene, pos, rot, parent);
            newItem.Register(isScene, isCulled);
            newItem.SetActive(true);
            return newItem;
        }

        private void SetEntityPosition(PrefabEntity newEntity, bool isScene, Vector3 pos, Quaternion rot, Transform parent) {
            newEntity.transform.position = pos;
            newEntity.transform.rotation = rot;
            newEntity.transform.SetParent(parent != null ? parent : null);
            //else {
            //    newEntity.transform.SetParent(isScene ? ActiveSceneTr : null);
            //}
        }

        private PrefabEntity CreateNewPrefab(PrefabEntity prefab) {
            GameObject newItem = Instantiate(prefab.gameObject);
            var prefabCopy = newItem.GetComponent<PrefabEntity>();
            prefabCopy.Setup();
            return prefabCopy;
        }

        private void ReturnToPool(GameObject targetGameObject) {
            if (targetGameObject == null || TimeManager.IsQuitting) {
                return;
            }
            if (!Application.isPlaying && !TimeManager.IsQuitting) {
                DestroyImmediate(targetGameObject);
                return;
            }
            var poolObj = targetGameObject.GetComponent<PrefabEntity>();
            if (poolObj == null) {
                Debug.Log("Error on pool object" + targetGameObject.name);
                Destroy(targetGameObject);
            }
            else {
                ReturnToPool(poolObj);
            }
        }

        private void ReturnToPool(PrefabEntity poolObj) {
            if (!Application.isPlaying && !TimeManager.IsQuitting || poolObj.PrefabId == 0 || poolObj.ObjectType == AssetType.Scene) {
                if (Application.isPlaying) {
                    poolObj.Unregister();
                }
                Debug.Log("Error on pool object" + poolObj.name);
                DestroyImmediate(poolObj.gameObject);
                return;
            }
            if (poolObj.Pooled) {
                return;
            }
            poolObj.Unregister();
            poolObj.SetActive(false);
            poolObj.transform.SetParent(PoolTransform);
            Queue<PrefabEntity> activeObjects = null;
            if (_pooledDict.TryGetValue(poolObj.PrefabId, out activeObjects)) {
                activeObjects.Enqueue(poolObj);
            }
            else {
                activeObjects = new Queue<PrefabEntity>();
                activeObjects.Enqueue(poolObj);
                _pooledDict.Add(poolObj.PrefabId, activeObjects);
            }
        }

        private int AddNewLoadedResource(string itemName) {
            if (string.IsNullOrEmpty(itemName)) {
                Debug.LogError(itemName + " is blank");
                return -1;
            }
            GameObject temp = SyncAddressables.LoadAsset<GameObject>(itemName + ".prefab");
            if (temp == null) {
                Debug.LogError("No resource located for " + itemName);
                return -1;
            }
            var entity = temp.GetComponent<PrefabEntity>();
            if (entity != null && entity.PrefabId != 0) {
                _referenceItems.Add(entity.PrefabId, entity);
                _stringToId.Add(itemName, entity.PrefabId);
                return entity.PrefabId;
            }
            entity.SetId(GetResourcePath(temp));
            _stringToId.Add(itemName, entity.PrefabId);
            _referenceItems.Add(entity.PrefabId, entity);
            return entity.PrefabId;
        }

        private PrefabEntity GetNewStringLoaded(int uniqueId) {
            if (_referenceItems.TryGetValue(uniqueId, out var prefab)) {
                return CreateNewPrefab(prefab);
            }
            Debug.LogError(uniqueId + " is not in reference dictionary");
            return null;
        }

        private PrefabEntity GetPooledEntity(int uniqueId) {
            if (!_pooledDict.TryGetValue(uniqueId, out var list) || list.Count == 0) {
                return null;
            }
            while (list.Count > 0) {
                var copy = list.Dequeue();
                if (copy != null) {
                    return copy;
                }
            }
            return null;
        }

        private const string ResourcesPath = "Resources/";

        private string GetResourcePath(GameObject prefab) {
#if UNITY_EDITOR
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(prefab);
            var resourceIndex = assetPath.IndexOf(ResourcesPath);
            if (resourceIndex != -1) {
                assetPath = assetPath.Substring(assetPath.IndexOf(ResourcesPath));
                assetPath = System.Text.RegularExpressions.Regex.Replace(assetPath, ResourcesPath, "");
            }
            return assetPath;
#else
            return new System.Guid().ToString();
#endif
        }

        //private void SetMain() {
        //    _main = this;
        //    Map.SceneObjects = _sceneObjects;
        //    IsQuitting = false;
        //    //for (int i = 0; i < AssetTypeExtension.MaxInt; i++) {
        //    //    _pooledDict.Add(i, new Dictionary<int, List<WorldEntity>>());
        //    //}
        //}
    }
}