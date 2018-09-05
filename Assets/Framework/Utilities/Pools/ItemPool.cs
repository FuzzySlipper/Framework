using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace PixelComrades {
    public class ItemPool : ScriptableSingleton<ItemPool> {

        private Transform _activeSceneTr = null;
        private Transform _inactiveSceneTr = null;

        private Dictionary<int, PrefabEntity> _referenceItems = new Dictionary<int, PrefabEntity>();
        private Dictionary<string, int> _stringToId = new Dictionary<string, int>();
        private Dictionary<int, Queue<PrefabEntity>> _pooledDict = new Dictionary<int, Queue<PrefabEntity>>();

        private static List<PrefabEntity> _sceneObjects = new List<PrefabEntity>();
        public static List<PrefabEntity> SceneObjects { get => _sceneObjects; }
        //private static ItemPool main {
        //    get {
        //        if (_main == null && !IsQuitting) {
        //            var scene = FindObjectOfType<ItemPool>();
        //            if (scene != null) {
        //                _main = scene;
        //            }
        //            else {
        //                GameObject pool = new GameObject("ItemPool");
        //                _main = pool.AddComponent<ItemPool>();
        //            }
        //            _main.SetMain();
        //        }
        //        return _main;
        //    }
        //}
        private static Transform PoolTransform {
            get {
                if (Main._inactiveSceneTr == null && !TimeManager.IsQuitting) {
                    Main._inactiveSceneTr = Game.GetMainChild("InactivePool");
                }
                return Main._inactiveSceneTr;
            }
        }
        public static Transform ActiveSceneTr {
            get {
                if (Main._activeSceneTr == null && !TimeManager.IsQuitting) {
                    Main._activeSceneTr = Game.GetMainChild("SceneObjects");
                    MessageKit.addObserver(Messages.LevelClear, DespawnScene);
                }
                return Main._activeSceneTr;
            }
        }

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
        }

        public static void ClearReferences() {
            Main._referenceItems.Clear();
            Main._stringToId.Clear();
            Resources.UnloadUnusedAssets();
        }

        public static T LoadAsset<T>(string location) where T : UnityEngine.Object {
#if UNITY_EDITOR
            //var path = string.Format("{0}{1}.{2}", UnityDirs.EditorFolder, location, AssetTypeExtensions.GetExtensionFromType<T>());
            //return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(string.Format("{0}{1}.{2}", UnityDirs.EditorFolder, location, AssetTypeExtensions.GetExtensionFromType<T>()));
#else
            return Resources.Load<T>(location);
#endif
        }

        public static PrefabEntity Spawn(string itemName, bool isSceneObject = false, bool isCulled = true) {
            return Main.SpawnByString(itemName, isSceneObject, Vector3.zero, Quaternion.identity, null, isCulled);
        }

        public static T Spawn<T>(string itemName, bool isSceneObject, bool isCulled) where T : MonoBehaviour {
            return GetWorldEntityComponent<T>(Main.SpawnByString(itemName, isSceneObject, Vector3.zero, Quaternion.identity, null, isCulled));
        }

        public static T Spawn<T>(string itemName, Vector3 position, Quaternion rotation, bool isScene, bool isCulled) where T : MonoBehaviour {
            return GetWorldEntityComponent<T>(Main.SpawnByString(itemName, isScene, position, rotation, null, isCulled));
        }

        public static T SpawnScenePrefab<T>(PrefabEntity targetObject, Vector3 pos, Quaternion rot) where T : MonoBehaviour {
            return GetWorldEntityComponent<T>(Main.GetPrefabCopy(targetObject, true, true, pos, rot));
        }

        public static PrefabEntity Spawn(PrefabEntity targetObject, bool isSceneObject = false, bool isCulled = false) {
            return Main.GetPrefabCopy(targetObject, isSceneObject, isCulled, Vector3.zero, Quaternion.identity);
        }

        public static PrefabEntity Spawn(PrefabEntity targetObject, Vector3 pos, Quaternion rot, bool isScene, bool isCulled) {
            return Main.GetPrefabCopy(targetObject, isScene, isCulled, pos, rot);
        }

        public static PrefabEntity Spawn(GameObject targetObject, Vector3 pos, Quaternion rot, bool isScene = true, bool isCulled = true) {
            return Main.GetPrefabCopy(targetObject.GetOrAddComponent<PrefabEntity>(), isScene, isCulled, pos, rot);
        }

        public static PrefabEntity SpawnScenePrefab(GameObject targetObject) {
            return Main.GetPrefabCopy(targetObject.GetOrAddComponent<PrefabEntity>(), true, true, Vector3.zero, Quaternion.identity);
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

        public static T SpawnUIPrefab<T>(PrefabEntity targetObject, Transform parent) where T : MonoBehaviour {
            var newObject = Main.GetPrefabCopy(targetObject, false, false, parent.position, parent.rotation, parent);
            newObject.transform.localScale = Vector3.one;
            return GetWorldEntityComponent<T>(newObject);
        }

        public static T SpawnUIPrefab<T>(string prefab, Transform parent) where T : MonoBehaviour {
            var newObject = Main.SpawnByString(prefab, false, parent.transform.position, parent.transform.rotation, parent, false);
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
            T returnType = item.GetComponent<T>();
            if (returnType == null) {
                returnType = item.gameObject.AddComponent<T>();
                item.RescanObject();
            }
            return returnType;
        }

        private PrefabEntity SpawnByString(string itemName, bool isScene, Vector3 pos, Quaternion rot, Transform parent, bool isCulled) {
            if (string.IsNullOrEmpty(itemName)) {
                Debug.LogError(itemName + " is blank");
                return null;
            }
            if (!Application.isPlaying) {
                GameObject temp = (GameObject) Resources.Load(itemName);
                var go = GameObject.Instantiate(temp, pos, rot);
                return go.GetOrAddComponent<PrefabEntity>();
            }
            int itemId;
            if (!_stringToId.TryGetValue(itemName, out itemId)) {
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
            if (prefab.PrefabId < 0) {
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
            if (parent != null) {
                newEntity.transform.SetParent(parent);
            }
            else {
                newEntity.transform.SetParent(isScene ? ActiveSceneTr : null);
            }
        }

        private PrefabEntity CreateNewPrefab(PrefabEntity prefab) {
            GameObject newItem = Instantiate(prefab.gameObject);
            var prefabCopy = SetNewPooledObject(newItem, prefab.PrefabId, prefab.ObjectType);
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
                Destroy(targetGameObject);
            }
            else {
                ReturnToPool(poolObj);
            }
        }

        private void ReturnToPool(PrefabEntity poolObj) {
            if (!Application.isPlaying && !TimeManager.IsQuitting || PoolTransform == null ||
                poolObj.PrefabId < 0 || poolObj.ObjectType == AssetType.Scene) {
                if (Application.isPlaying) {
                    poolObj.Unregister();
                }
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

        private PrefabEntity SetNewPooledObject(GameObject pooledObj, int id, AssetType type) {
            return pooledObj.GetOrAddComponent<PrefabEntity>();
        }

        private int AddNewLoadedResource(string itemName) {
            if (string.IsNullOrEmpty(itemName)) {
                Debug.LogError(itemName + " is blank");
                return -1;
            }
            GameObject temp = (GameObject) Resources.Load(itemName);
            if (temp == null) {
                Debug.LogError("No resource located for " + itemName);
                return -1;
            }
            var entity = temp.GetComponent<PrefabEntity>();
            if (entity != null && entity.PrefabId >= 0) {
                _referenceItems.Add(entity.PrefabId, entity);
                _stringToId.Add(itemName, entity.PrefabId);
                return entity.PrefabId;
            }
            entity = temp.GetOrAddComponent<PrefabEntity>();
            entity.SetId(GetResourcePath(temp));
            _stringToId.Add(itemName, entity.PrefabId);
            _referenceItems.Add(entity.PrefabId, entity);
            return entity.PrefabId;
        }

        private PrefabEntity GetNewStringLoaded(int uniqueId) {
            PrefabEntity prefab;
            if (_referenceItems.TryGetValue(uniqueId, out prefab)) {
                return CreateNewPrefab(prefab);
                //var pooledObject = Instantiate(prefab);
                //return SetNewPooledObject(pooledObject, uniqueId, AssetType.Prefab);
            }
            Debug.LogError(uniqueId + " is not in reference dictionary");
            return null;
        }

        private PrefabEntity GetPooledEntity(int uniqueId) {
            Queue<PrefabEntity> list;
            if (!_pooledDict.TryGetValue(uniqueId, out list) || list.Count == 0) {
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