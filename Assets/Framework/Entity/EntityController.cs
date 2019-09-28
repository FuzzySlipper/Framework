using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
using Random = System.Random;

namespace PixelComrades {
    public static class EntityController {
        private static ManagedArray<Entity> _entities = new ManagedArray<Entity>(200);
        private static Dictionary<Type, ManagedArray> _components = new Dictionary<Type, ManagedArray>();
        private static Dictionary<Type, List<NodeFilter>> _nodeFilters = new Dictionary<Type, List<NodeFilter>>();
        private static Dictionary<Type, List<EventReceiverFilter>> _eventFilters = new Dictionary<Type, List<EventReceiverFilter>>();
        private static Dictionary<Type, NodeFilter> _filterHandler = new Dictionary<Type, NodeFilter>();
        private static System.Type[] _tempRemoveList = new Type[100];

        public static ManagedArray<Entity> EntitiesArray { get => _entities; }

        public static void RegisterNodeFilter(NodeFilter filter, System.Type handleType) {
            for (int i = 0; i < filter.RequiredTypes.Length; i++) {
                var type = filter.RequiredTypes[i];
                _nodeFilters.GetOrAdd(type).Add(filter);
            }
            if (_filterHandler.ContainsKey(handleType)) {
                _filterHandler[handleType] = filter;
            }
            else {
                _filterHandler.Add(handleType, filter);
            }
        }

        public static T GetNode<T>(this Entity entity) where T : class, INode, new() {
            if (entity == null) {
                return null;
            }
            var type = typeof(T);
            return !_filterHandler.TryGetValue(type, out var filter) ? null : ((NodeFilter<T>) filter).GetNode(entity);
        }

        public static NodeList<T> GetNodeList<T>() where T : class, INode, new() {
            var type = typeof(T);
            return !_filterHandler.TryGetValue(type, out var filter) ? null : ((NodeFilter<T>) filter).AllNodes;
        }

        public static ComponentArray<T> GetComponentArray<T>() where T : IComponent {
            if (_components.TryGetValue(typeof(T), out var list)) {
                return (ComponentArray<T>) list;
            }
            if (typeof(T).IsInterface) {
                Debug.LogErrorFormat("Retrieve interface {0} instead of class ", typeof(T));
            }
            return AddComponentArray<T>();
        }

        public static IComponentArray GetComponentArray(System.Type type) {
            if (_components.TryGetValue(type, out var list)) {
                return list as IComponentArray;
            }
            return null;
        }

        public static void RegisterReceiver(EventReceiverFilter filter) {
            for (int i = 0; i < filter.Types.Length; i++) {
                var type = filter.Types[i];
                if (!_eventFilters.TryGetValue(type, out var list)) {
                    list = new List<EventReceiverFilter>();
                    _eventFilters.Add(type, list);
                }
                list.Add(filter);
            }
        }

        //public static T GetComponent<T>(this ComponentReference reference) where T : IComponent {
        //    return _components.TryGetValue(reference.Type, out var list) ?  ((ManagedArray<T>) list)[reference.Index] : default(T);
        //}

        public static ComponentReference? GetComponentReference(this Entity entity, Type type) {
            if (entity == null) {
                return null;
            }
            return entity.TryGetReference(type, out var cRef)? cRef : (ComponentReference?) null;
        }

        public static bool HasComponent<T>(this Entity entity) {
            if (entity == null) {
                return false;
            }
            return entity.HasReference<T>();
        }

        public static Entity Get(int index) {
            if (index < 0) {
                return null;
            }
            return _entities[index];
        }

        public static Entity GetEntity(int index) {
            if (index < 0) {
                return null;
            }
            return _entities[index];
        }

        public static int AddEntityToMainList(Entity entity) {
            return _entities.Add(entity);
        }

        public static int AddEntityToMainList(Entity entity, int index) {
            if (_entities.IndexFree(index)) {
                _entities.Set(index, entity);
                return index;
            }
            return _entities.Add(entity);
        }

        public static void FinishDeleteEntity(Entity entity) {
            _entities.Remove(entity.Id);
            entity.CopyKeys(ref _tempRemoveList);
            var limit = entity.ComponentCount;
            for (int i = 0; i < limit; i++) {
                entity.Remove(_tempRemoveList[i]);
            }
        }

        private static ComponentArray<T> AddComponentArray<T>() where T : IComponent {
            var componentList = new ComponentArray<T>();
            _components.Add(typeof(T), componentList);
            return componentList;
        }

        //no more multiple components on an entity, might need to check for Container<T> when getting <T> and null
        public static T Add<T>(this Entity entity, T newComponent) where T : IComponent {
            if (entity == null) {
                return default(T);
            }
            var type = typeof(T);
            var array = GetComponentArray<T>();
            array.Add(entity, newComponent);
            if (_nodeFilters.TryGetValue(type, out var filterList)) {
                for (int i = 0; i < filterList.Count; i++) {
                    filterList[i].TryAdd(entity);
                }
            }
            if (_eventFilters.TryGetValue(type, out var receiverList)) {
                for (int i = 0; i < receiverList.Count; i++) {
                    receiverList[i].CheckAdd(entity);
                }
            }
            return newComponent;
        }

        public static T Find<T>(this Entity entity) where T : IComponent {
            if (entity == null) {
                return default(T);
            }
            var checkEntity = entity;
            _loopLimiter.Reset();
            var type = typeof(T);
            while (_loopLimiter.Advance()) {
                if (checkEntity == null) {
                    break;
                }
                if (checkEntity.HasReference(type)) {
                    return checkEntity.Get<T>();
                }
                checkEntity = checkEntity.GetParent();
            }
            return default(T);
        }

        public static Entity FindEntityWith<T>(this Entity entity) where T : IComponent {
            if (entity == null) {
                return null;
            }
            var checkEntity = entity;
            var type = typeof(T);
            while (checkEntity != null) {
                if (checkEntity.HasReference(type)) {
                    return checkEntity;
                }
                checkEntity = checkEntity.GetParent();
            }
            return null;
        }

        public static T GetSelfOrParent<T>(this Entity entity) where T : IComponent {
            if (entity == null) {
                return default(T);
            }
            var parent = entity.GetParent();
            if (parent == null) {
                return entity.Get<T>();
            }
            return !entity.HasReference(typeof(T)) ? parent.Get<T>() : entity.Get<T>();
        }

        public static System.Object GetComponent(Entity entity, string type) {
            return GetComponent(entity, ParseUtilities.ParseType(type));
        }

        public static System.Object GetComponent(Entity entity, System.Type type) {
            if (entity == null) {
                return null;
            }
            if (entity.TryGetReference(type, out var cref)) {
                return cref.Get();
            }
            return null;
        }

        public static T Get<T>(this Entity entity) where T : IComponent {
            if (entity == null) {
                return default(T);
            }
            if (entity.TryGetReference(typeof(T), out var cref)) {
                return cref.Get<T>();
            }
//            if (_components.TryGetValue(typeof(T), out var array)) {
//                var typeArray = ((ComponentArray<T>) array);
//                if (typeArray.TryGet(entity, out var value)) {
//                    return value;
//                }
//            }
            return default(T);
        }

        public static T GetOrAdd<T>(this Entity entity) where T : IComponent, new() {
            if (entity == null) {
                return default(T); 
            }
//            var array = GetComponentArray<T>();
//            if (array.HasComponent(entity)) {
//                return array.Get(entity);
//            }    
            if (entity.TryGetReference(typeof(T), out var cref)) {
                return cref.Get<T>();
            }
            return entity.Add(new T());
        }

        //public static List<T> GetAllMatching<T>(this Entity entity) where T : IComponent {
        //    if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
        //        return null;
        //    }
        //    var targetType = typeof(T);
        //    List<T> returnList = new List<T>();
        //    foreach (var cref in entityList) {
        //        var compareType = cref.Key;
        //        if (!targetType.IsAssignableFrom(compareType) || !compareType.IsClass) {
        //            continue;
        //        }
        //        returnList.Add((T) cref.Value.Get());
        //    }
        //    return returnList.Count > 0 ? returnList : null;
        //}

        public static void Remove<T>(this Entity entity, T component) where T : IComponent {
            entity.Remove(typeof(T));
        }

        public static void Remove<T>(this Entity entity) where T : IComponent {
            entity.Remove(typeof(T));
        }

        public static void Remove(this Entity entity, System.Type type){
            if (!entity.TryGetReference(type, out var cref)) {
                return;
            }
            Remove(entity, cref, type);
        }

        private static void Remove(Entity entity, ComponentReference reference, System.Type type) {
            var componentArray = GetComponentArray(type);
            if (componentArray == null) {
                return;
            }
            componentArray.RemoveByEntity(entity);
            entity.RemoveReference(reference);
            if (_nodeFilters.TryGetValue(type, out var filterList)) {
                for (int f = 0; f < filterList.Count; f++) {
                    filterList[f].CheckRemove(entity);
                }
            }
            if (_eventFilters.TryGetValue(type, out var receiverList)) {
                for (int r = 0; r < receiverList.Count; r++) {
                    receiverList[r].CheckRemove(entity);
                }
            }
        }
        public static Entity GetParentOrSelf(this Entity entity) {
            if (entity.ParentId < 0) {
                return entity;
            }
            var parent = GetEntity(entity.ParentId);
            return parent ?? entity;
        }

        public static Entity GetParent(this Entity entity) {
            if (entity == null) {
                return null;
            }
            return entity.ParentId < 0 ? null : GetEntity(entity.ParentId);
        }

        private static WhileLoopLimiter _loopLimiter = new WhileLoopLimiter(5000);

        public static Entity GetRoot(this Entity entity) {
            if (entity == null) {
                return null;
            }
            if (entity.ParentId < 0) {
                return entity;
            }
            _loopLimiter.Reset();
            var root = entity;
            while (_loopLimiter.Advance()) {
                var newRoot = GetEntity(root.ParentId);
                if (newRoot != null) {
                    root = newRoot;
                }
                else {
                    break;
                }
            }
            return root;
        }

        public static Vector3 GetPosition(this Entity entity) {
            if (entity == null) {
                return Vector3.zero;
            }
            var checkEntity = entity;
            while (checkEntity != null) {
                var tr = checkEntity.Get<TransformComponent>();
                if (tr != null && tr.IsValid) {
                    return tr.position;
                }
                checkEntity = checkEntity.GetParent();
            }
            return Vector3.zero;
        }

        public static Quaternion GetRotation(this Entity entity) {
            if (entity == null) {
                return Quaternion.identity;
            }
            var tr = entity.Get<TransformComponent>();
            if (tr != null && tr.IsValid) {
                return tr.rotation;
            }
            return Quaternion.identity;
        }

        public static float GetMoveSpeed(this Entity entity) {
            if (entity.HasComponent<MoveSpeed>()) {
                return entity.Get<MoveSpeed>().Speed;
            }
            return 1;
        }

        public static float Distance(this Entity entity, Transform other) {
            var firstPosition = entity.GetPosition();
            var secondPosition = other.position;
            Vector3 heading = firstPosition - secondPosition;
            return heading.magnitude;
        }

        public static bool IsDead(this Entity entity) {
            if (entity == null) {
                return false;
            }
            return entity.Tags.Contain(EntityTags.IsDead);
        }

        public static T FindStat<T>(this Entity entity, string statFullID) where T : BaseStat {
            WhileLoopLimiter.ResetInstance();
            var currentEntity = entity;
            while (WhileLoopLimiter.InstanceAdvance()) {
                if (currentEntity == null) {
                    return null;
                }
                var stats = currentEntity.Get<StatsContainer>();
                var stat = stats.Get<T>(statFullID);
                if (stat != null) {
                    return stat;
                }
                currentEntity = currentEntity.GetParent();
            }
            return null;
        }

        public static float FindStatValue(this Entity entity, string statFullID) {
            WhileLoopLimiter.ResetInstance();
            var currentEntity = entity;
            while (WhileLoopLimiter.InstanceAdvance()) {
                if (currentEntity == null) {
                    return 0f;
                }
                var stats = currentEntity.Get<StatsContainer>();
                var stat = stats.Get(statFullID);
                if (stat != null) {
                    return stat.Value;
                }
                currentEntity = currentEntity.GetParent();
            }
            return 0f;
        }

        public static bool FindStat<T>(this Entity entity, string statFullID, System.Action<T> del) where T : BaseStat {
            WhileLoopLimiter.ResetInstance();
            var currentEntity = entity;
            while (WhileLoopLimiter.InstanceAdvance()) {
                if (currentEntity == null) {
                    return false;
                }
                var stats = currentEntity.Get<StatsContainer>();
                if (stats.HasStat(statFullID)) {
                    var stat = stats.Get<T>(statFullID);
                    if (stat != null) {
                        del(stat);
                        return true;
                    }
                }
                currentEntity = currentEntity.GetParent();
            }
            return false;
        }

        public static T FindNode<T>(this Entity entity) where T : class, INode, new() {
            var type = typeof(T);
            if (!_filterHandler.TryGetValue(type, out var filter)) {
                return null;
            } 
            var node = ((NodeFilter<T>) filter).GetNode(entity);
            if (node != null) {
                return node;
            }
            var parent = entity.GetParent();
            while (parent != null) {
                node = ((NodeFilter<T>) filter).GetNode(parent);
                if (node != null) {
                    return node;
                }
                parent = parent.GetParent();
            }
            return null;
        }
    }
}
