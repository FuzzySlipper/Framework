using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;

namespace PixelComrades {
    public class ItemPool : ScriptableSingleton<ItemPool> {

        //private Transform _activeSceneTr = null;
        private Transform _inactiveSceneTr = null;

        private Dictionary<string, Queue<PrefabEntity>> _pooledDict = new Dictionary<string, Queue<PrefabEntity>>();
        private Dictionary<GameObjectReference, string> _prefabKeys = new Dictionary<GameObjectReference, string>();
        
        private static GenericPool<GenericLoadOperation> _loadPool = new GenericPool<GenericLoadOperation>(5, null);
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
            foreach (var prefabKey in Main._prefabKeys) {
                // foreach (var l in Addressables.ResourceLocators) {
                //     if (l.Locate(prefabKey.Key, prefabKey.GetType(), out var locs)) {
                //         locs[0].PrimaryKey
                //     }
                // }
                prefabKey.Key.ReleaseAsset();
            }
            Main._prefabKeys.Clear();
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

        public static void Spawn<T>(T loadOp) where T : LoadOperationEvent {
            Main.SpawnPrefabCopy(loadOp);
        }

        public static void Spawn(GameObjectReference targetObject, Action<PrefabEntity> action) {
            var loader = _loadPool.New();
            loader.Set(targetObject, action);
            Main.SpawnPrefabCopy(loader);
        }
        
        public static GameObject SpawnScenePrefab(GameObject targetObject, Vector3 pos, Quaternion rot) {
            return Main.GetPrefabCopy(targetObject.GetOrAddComponent<PrefabEntity>(), true, true, pos, rot).gameObject;
        }

        public static PrefabEntity Spawn(PrefabEntity targetObject, bool isSceneObject = true, bool isCulled = true) {
            return Main.GetPrefabCopy(targetObject, isSceneObject, isCulled, Vector3.zero, Quaternion.identity);
        }

        public static PrefabEntity Spawn(GameObject targetObject, bool isSceneObject = true, bool isCulled = true) {
            return Main.GetPrefabCopy(targetObject.GetOrAddComponent<PrefabEntity>(), isSceneObject, isCulled, Vector3.zero, Quaternion.identity);
        }

        public static T Spawn<T>(GameObject targetObject, Vector3 pos, Quaternion rot, bool isSceneObject, bool isCulled) where T : MonoBehaviour {
            return GetWorldEntityComponent<T>(Main.GetPrefabCopy(targetObject.GetOrAddComponent<PrefabEntity>(), isSceneObject, isCulled, pos, rot));
        }

        public static GameObject SpawnUIPrefab(PrefabEntity targetObject, Transform parent) {
            var newObject = Main.GetPrefabCopy(targetObject, false, false, parent.position, parent.rotation, parent);
            newObject.transform.localScale = Vector3.one;
            return newObject.gameObject;
        }

        public static T SpawnUIPrefab<T>(GameObject targetObject, Transform parent) where T : MonoBehaviour {
            var newObject = Main.GetPrefabCopy(targetObject.GetOrAddComponent<PrefabEntity>(), false, false, parent.position, parent.rotation, parent);
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

        private void SpawnPrefabCopy<T>(T loadOp) where T : LoadOperationEvent {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                loadOp.NewPrefab = ((GameObject) UnityEditor.PrefabUtility.InstantiatePrefab(loadOp.SourcePrefab.Asset))
                .GetOrAddComponent<PrefabEntity>();
                loadOp.OnComplete();
                return;
            }
#endif
            if (!_prefabKeys.TryGetValue(loadOp.SourcePrefab, out var key)) {
                loadOp.SourcePrefab.LoadAsset(handle => {
                    var result = handle;
                    if (result == null) {
                        Debug.LogErrorFormat("No Prefab at {0}", loadOp.SourcePrefab.Asset != null? loadOp.SourcePrefab.Asset.name : "null");
                        return;
                    }
                    var prefab = result.GetOrAddComponent<PrefabEntity>();
                    if (prefab.IdInvalid) {
                        prefab.SetId(loadOp.SourcePrefab.AssetReference.AssetGUID);
                    }
                    _prefabKeys.AddOrUpdate(loadOp.SourcePrefab, prefab.Guid);
                    SpawnPrefabCopy(loadOp);
                });
                return;
            }
            loadOp.NewPrefab = GetPooledEntity(key);
            if (loadOp.NewPrefab == null) {
                loadOp.NewPrefab = CreateNewPrefab(loadOp.SourcePrefab.LoadedAsset);
            }
            if (loadOp.NewPrefab == null) {
                loadOp.OnComplete();
                return;
            }
            loadOp.NewPrefab.transform.SetParent(null);
            loadOp.NewPrefab.Register(true, true);
            loadOp.NewPrefab.SetActive(true);
            loadOp.OnComplete();
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
            if (prefab.IdInvalid) {
                prefab.SetId(ref _pooledDict, null);
            }
            PrefabEntity newItem = GetPooledEntity(prefab.Guid);
            if (newItem == null) {
                newItem = CreateNewPrefab(prefab.gameObject);
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

        private PrefabEntity CreateNewPrefab(GameObject prefab) {
            if (prefab == null) {
                return null;
            }
            GameObject newItem = Instantiate(prefab);
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
            if (!Application.isPlaying && !TimeManager.IsQuitting || poolObj.IdInvalid || poolObj.ObjectType == AssetType.Scene) {
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
            if (_pooledDict.TryGetValue(poolObj.Guid, out activeObjects)) {
                activeObjects.Enqueue(poolObj);
            }
            else {
                activeObjects = new Queue<PrefabEntity>();
                activeObjects.Enqueue(poolObj);
                _pooledDict.Add(poolObj.Guid, activeObjects);
            }
        }

        private PrefabEntity GetPooledEntity(string uniqueId) {
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

        private class GenericLoadOperation : LoadOperationEvent {
            public Action<PrefabEntity> Del;

            public void Set(GameObjectReference reference, Action<PrefabEntity> del) {
                Del = del;
                SourcePrefab = reference;
            }

            public override void OnComplete() {
                Del(NewPrefab);
                Clear();
                ItemPool._loadPool.Store(this);
            }

            public void Clear() {
                Del = null;
                NewPrefab = null;
                SourcePrefab = null;
            }
        }
    }

    public abstract class LoadOperationEvent {
        public GameObjectReference SourcePrefab;
        public PrefabEntity NewPrefab;
        public abstract void OnComplete();
    }

}