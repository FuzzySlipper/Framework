using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [DisallowMultipleComponent] [IgnoreFileSerialization]
    public class PrefabEntity : EntityIdentifier {

        [SerializeField, HideInInspector] private SerializedMetaData _metadata = new SerializedMetaData();
        [SerializeField] private AssetType _objectType = AssetType.Prefab;
        [SerializeField] private bool _pooled = false;
        [SerializeField] private string _resourcePath = "";
        [SerializeField] private int _hashId = -1;

        private ISystemFixedUpdate[] _systemFixedUpdate;
        private ISystemUpdate[] _systemUpdate;
        private ITurnUpdate[] _turnUpdate;
        private IPoolEvents[] _poolListeners;
        private Renderer[] _renderers;
        private Light[] _lights;
        private bool _active = true;
        private bool _isSceneObject = false;
        private Transform _transform;
        private bool _isCulled = false;

        public bool Pooled { get { return _pooled; } }
        public int PrefabId { get { return _hashId; } }
        public string ResourcePath { get => _resourcePath; }
        public AssetType ObjectType { get { return _objectType; } }
        public bool IsSceneObject { get { return _isSceneObject; } } // careful using this
        public Point3 SectorPosition { get; set; }
        public SerializedMetaData Metadata { get { return _metadata; } set { _metadata = value; } }
        public Renderer[] Renderers { get { return _renderers; } }
        public bool SceneActive { get { return _active; } }
        public Transform Transform {
            get {
                if (_transform == null) {
                    _transform = transform;
                }
                return _transform;
            }
        }

        public void SetStatic() {
            _hashId = -1;
            _objectType = AssetType.Scene;
            ScanObject();
            CheckCreate();
            Register(true, false);
            RegisterInterfaces(true);
        }

        public void SetId(ref HashSet<int> ids, string path) {
            _resourcePath = path;
            _hashId = _resourcePath.GetHashCode();
            while (ids.Contains(_hashId)) {
                _hashId = _resourcePath.GetHashCode();
            }
            ids.Add(_hashId);
        }

        public void SetId(string path) {
            _resourcePath = path;
            _hashId = _resourcePath.GetHashCode();
        }

        public void Setup() {
            ScanObject();
            CheckCreate();
        }

        private void CheckCreate() {
            var onCreate = transform.GetComponentsInChildren<IOnCreate>(true);
            if (onCreate == null || onCreate.Length == 0) {
                return;
            }
            for (int i = 0; i < onCreate.Length; i++) {
                onCreate[i].OnCreate(this);
            }
        }

        private void ScanObject() {
            _systemFixedUpdate = GetComponentsInChildren<ISystemFixedUpdate>(true);
            _systemUpdate = GetComponentsInChildren<ISystemUpdate>(true);
            _poolListeners = GetComponentsInChildren<IPoolEvents>(true);
            _turnUpdate = GetComponentsInChildren<ITurnUpdate>(true);
            _lights = GetComponentsInChildren<Light>(true);
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        public Bounds MaxBounds() {
            var bound = new Bounds(transform.position, Vector3.zero);
            if (_renderers == null || _renderers.Length == 0) {
                return bound;
            }
            for (int i = 0; i < _renderers.Length; i++) {
                bound.Encapsulate(_renderers[i].bounds);
            }
            var localCenter = bound.center - transform.position;
            bound.center = transform.TransformPoint(localCenter);
            return bound;
        }

        public void OverrideSceneObject(bool newStatus) {
            if (_isSceneObject == newStatus) {
                return;
            }
            _isSceneObject = newStatus;
            if (!_isSceneObject) {
                ItemPool.SceneObjects.Remove(this);
                World.Get<CullingManager>().Remove(this);
            }
            else {
                ItemPool.SceneObjects.Add(this);
                World.Get<CullingManager>().Add(this);
            }
        }

        public virtual void Register(bool isSceneObject, bool isCulled) {
            if (!Application.isPlaying || !Pooled) {
                return;
            }
            _pooled = false;
            _isSceneObject = isSceneObject;
            if (_poolListeners != null) {
                for (int i = 0; i < _poolListeners.Length; i++) {
                    if (_poolListeners[i] == null) {
                        continue;
                    }
                    _poolListeners[i].OnPoolSpawned();
                }
            }
            if (_isSceneObject) {
                ItemPool.SceneObjects.Add(this);
            }
            _isCulled = isCulled;
            if (isCulled) {
                World.Get<CullingManager>().Add(this);
            }
        }

        public void Unregister() {
            if (!Application.isPlaying || Pooled) {
                return;
            }
            _pooled = true;
            if (_isSceneObject) {
                ItemPool.SceneObjects.Remove(this);
            }
            if (_isCulled) {
                World.Get<CullingManager>().Remove(this);
            }
            _isSceneObject = false;
            if (_poolListeners != null) {
                for (int i = 0; i < _poolListeners.Length; i++) {
                    if (_poolListeners[i] == null) {
                        continue;
                    }
                    _poolListeners[i].OnPoolDespawned();
                }
            }
        }

        public virtual void SetActive(bool status) {
            if (gameObject == null) {
                return;
            }
            gameObject.SetActive(status);
            if (!Application.isPlaying) {
                return;
            }
            RegisterInterfaces(status);
        }

        private void RegisterInterfaces(bool status) {
            _active = status;
            if (_systemFixedUpdate != null) {
                for (int i = 0; i < _systemFixedUpdate.Length; i++) {
                    if (_systemFixedUpdate[i] == null) {
                        continue;
                    }
                    if (status) {
                        SystemManager.AddFixed(_systemFixedUpdate[i]);
                    }
                    else {
                        SystemManager.Remove(_systemFixedUpdate[i]);
                    }
                }
            }
            if (_systemUpdate != null) {
                for (int i = 0; i < _systemUpdate.Length; i++) {
                    if (_systemUpdate[i] == null) {
                        continue;
                    }
                    if (status) {
                        SystemManager.Add(_systemUpdate[i]);
                    }
                    else {
                        SystemManager.Remove(_systemUpdate[i]);
                    }
                }
            }
            if (_turnUpdate != null) {
                for (int i = 0; i < _turnUpdate.Length; i++) {
                    if (_turnUpdate[i] == null) {
                        continue;
                    }
                    if (status) {
                        SystemManager.AddTurn(_turnUpdate[i]);
                    }
                    else {
                        SystemManager.RemoveTurn(_turnUpdate[i]);
                    }
                }
            }
        }

        public void SetVisible(bool status) {
            if (_renderers != null) {
                for (var i = 0; i < _renderers.Length; i++) {
                    _renderers[i].enabled = status;
                }
            }
            if (_lights != null) {
                for (int i = 0; i < _lights.Length; i++) {
                    _lights[i].enabled = status;
                }
            }
        }

        public MaterialPropertyBlock[] GatherMatBlocks() {
            var blocks = new MaterialPropertyBlock[_renderers.Length];
            for (int i = 0; i < blocks.Length; i++) {
                blocks[i] = new MaterialPropertyBlock();
                _renderers[i].GetPropertyBlock(blocks[i]);
            }
            return blocks;
        }

        public static int GetStableHashCode(string str) {
            unchecked {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length && str[i] != '\0'; i += 2) {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i + 1] == '\0')
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

        public static void RegisterNonPooled(GameObject nonPooled, bool status) {
            if (status) {
                var onCreate = nonPooled.GetComponentsInChildren<IOnCreate>();
                if (onCreate != null) {
                    for (int i = 0; i < onCreate.Length; i++) {
                        onCreate[i].OnCreate(null);
                    }
                }
            }
            var systemFixedUpdate = nonPooled.GetComponentsInChildren<ISystemFixedUpdate>(true);
            var systemUpdate = nonPooled.GetComponentsInChildren<ISystemUpdate>(true);
            var poolListeners = nonPooled.GetComponentsInChildren<IPoolEvents>(true);
            var turnUpdate = nonPooled.GetComponentsInChildren<ITurnUpdate>(true);
            if (poolListeners != null) {
                for (int i = 0; i < poolListeners.Length; i++) {
                    if (status) {
                        poolListeners[i].OnPoolSpawned();
                    }
                    else {
                        poolListeners[i].OnPoolDespawned();
                    }
                }
            }
            if (systemFixedUpdate != null) {
                for (int i = 0; i < systemFixedUpdate.Length; i++) {
                    if (status) {
                        SystemManager.AddFixed(systemFixedUpdate[i]);
                    }
                    else {
                        SystemManager.Remove(systemFixedUpdate[i]);
                    }
                }
            }
            if (systemUpdate != null) {
                for (int i = 0; i < systemUpdate.Length; i++) {
                    if (status) {
                        SystemManager.Add(systemUpdate[i]);
                    }
                    else {
                        SystemManager.Remove(systemUpdate[i]);
                    }
                }
            }
            if (turnUpdate != null) {
                for (int i = 0; i < turnUpdate.Length; i++) {
                    if (status) {
                        SystemManager.AddTurn(turnUpdate[i]);
                    }
                    else {
                        SystemManager.RemoveTurn(turnUpdate[i]);
                    }
                }
            }
        }
    }
}