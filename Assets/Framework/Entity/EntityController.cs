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
        private static Dictionary<int, Dictionary<Type, ComponentReference>> _entityComponents = new Dictionary<int, Dictionary<Type, ComponentReference>>();
        private static Dictionary<Type, List<NodeFilter>> _filtersToCheck = new Dictionary<Type, List<NodeFilter>>();
        private static Dictionary<Type, NodeFilter> _filterHandler = new Dictionary<Type, NodeFilter>();
        private static System.Type[] _tempRemoveList = new Type[100];

        public static ManagedArray<Entity> EntitiesArray { get => _entities; }

        public static void RegisterNodeFilter(NodeFilter filter, System.Type handleType) {
            for (int i = 0; i < filter.RequiredTypes.Length; i++) {
                var type = filter.RequiredTypes[i];
                _filtersToCheck.GetOrAdd(type).Add(filter);
            }
            if (_filterHandler.ContainsKey(handleType)) {
                _filterHandler[handleType] = filter;
            }
            else {
                _filterHandler.Add(handleType, filter);
            }
        }

        public static T GetNode<T>(this Entity entity) where T : class, INode, new() {
            var type = typeof(T);
            return !_filterHandler.TryGetValue(type, out var filter) ? null : ((NodeFilter<T>) filter).GetNode(entity);
        }

        public static List<T> GetNodeList<T>() where T : class, INode, new() {
            var type = typeof(T);
            return !_filterHandler.TryGetValue(type, out var filter) ? null : ((NodeFilter<T>) filter).AllNodes;
        }

        public static ManagedArray<T> GetComponentArray<T>() where T : IComponent {
            return _components.TryGetValue(typeof(T), out var list) ? (ManagedArray<T>) list : null;
        }

        public static ManagedArray GetGenericComponentArray(System.Type type) {
            return _components.TryGetValue(type, out var list) ? list : null;
           
        }

        public static Dictionary<Type, ComponentReference> GetEntityComponentDict(int entity) {
            return _entityComponents.TryGetValue(entity, out var dict) ? dict : null;
        }

        //public static T GetComponent<T>(this ComponentReference reference) where T : IComponent {
        //    return _components.TryGetValue(reference.Type, out var list) ?  ((ManagedArray<T>) list)[reference.Index] : default(T);
        //}

        public static ComponentReference? GetComponentReference(this Entity entity, Type type) {
            if (entity == null) {
                return null;
            }
            if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
                return null;
            }
            return entityList.TryGetValue(type, out var cRef) ? cRef : (ComponentReference?) null;
        }

        public static bool HasComponent<T>(this Entity entity) {
            if (entity == null) {
                return false;
            }
            var type = typeof(T);
            if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
                return false;
            }
            return entityList.ContainsKey(type);
        }

        public static Entity GetEntity(this IComponent component) {
            if (component == null) {
                return null;
            }
            return GetEntity(component.Owner);
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

        public static void FinishDeleteEntity(Entity entity) {
            _entities.Remove(entity.Id);
            if (!_entityComponents.TryGetValue(entity, out var componentList)) {
                return;
            }
            componentList.Keys.CopyTo(_tempRemoveList, 0);
            var limit = componentList.Count;
            for (int i = 0; i < limit; i++) {
                entity.Remove(_tempRemoveList[i]);
            }
            componentList.Clear();
            _entityComponents.Remove(entity.Id);
        }

        //no more multiple components on an entity, might need to check for Container<T> when getting <T> and null
        public static T Add<T>(this Entity entity, T newComponent) where T : IComponent {
            if (entity == null) {
                return default(T);
            }
            var type = typeof(T);
            if (!_components.TryGetValue(type, out var componentList)) {
                componentList = new ManagedArray<T>();
                _components.Add(type, componentList);
            }
            newComponent.Owner = entity;
            if (!_entityComponents.TryGetValue(entity.Id, out var entityComponents)) {
                entityComponents = new Dictionary<Type, ComponentReference>();
                _entityComponents.Add(entity.Id, entityComponents);
            }
            if (entityComponents.TryGetValue(type, out var cref)) {
                var index = cref.Index;
                if (!type.IsValueType && ((ManagedArray<T>) componentList)[index] is IReceive receiveOld) {
                    entity.RemoveObserver(receiveOld);
                }
                entityComponents[type] = new ComponentReference(index, componentList);
                ((ManagedArray<T>) componentList)[index] = newComponent;
                if (type.IsValueType) {
                    return newComponent;
                }
            }
            else {
                var index = ((ManagedArray<T>) componentList).Add(newComponent);
                entityComponents.Add(type, new ComponentReference(index, componentList));
            }
            if (newComponent is IReceive receive) {
                if (type.IsValueType) {
                    Debug.LogErrorFormat("Error: cannot have event receivers on value type {0}", type.Name);
                }
                else {
                    entity.AddObserver(receive);
                }
            }
            if (_filtersToCheck.TryGetValue(type, out var filterList)) {
                for (int i = 0; i < filterList.Count; i++) {
                    filterList[i].TryAdd(entity, entityComponents);
                }
            }
            return newComponent;
        }

        public static bool Find<T>(this Entity entity, Action<T> del) where T : IComponent {
            if (entity == null) {
                return false;
            }
            if (entity.HasComponent<T>()) {
                return entity.Get<T>(del);
            }
            var parent = entity.GetParent();
            _loopLimiter.Reset();
            var type = typeof(T);
            while (_loopLimiter.Advance()) {
                if (parent == null) {
                    break;
                }
                _entityComponents.TryGetValue(parent.Id, out var parentList);
                if (parentList != null && parentList.ContainsKey(type)) {
                    return parent.Get<T>(del);
                }
                //if (parentList != null && parentList.ContainsDerivedType(type)) {
                //    return parent.GetDerived<T>(del);
                //}
                parent = parent.GetParent();
            }
            return false;
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
                _entityComponents.TryGetValue(checkEntity.Id, out var componentList);
                if (componentList != null && componentList.ContainsKey(type)) {
                    return checkEntity.Get<T>();
                }
                //if (componentList != null && componentList.ContainsDerivedType(type)) {
                //    return checkEntity.GetDerived<T>();
                //}
                checkEntity = checkEntity.GetParent();
            }
            return default(T);
        }

        public static T GetSelfOrParent<T>(this Entity entity) where T : IComponent {
            if (entity == null) {
                return default(T);
            }
            var parent = entity.GetParent();
            if (parent == null) {
                return entity.Get<T>();
            }
            if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
                return parent.Get<T>();
            }
            var type = typeof(T);
            if (entityList.ContainsKey(type)) {
                return entity.Get<T>();
            }
            return parent.Get<T>();
        }

        public static T Get<T>(this IComponent component) where T : IComponent {
            if (component == null) {
                return default(T);
            }
            return GetEntity(component.Owner).Get<T>();
        }

        public static System.Object GetComponent(Entity entity, string type) {
            return GetComponent(entity, ParseUtilities.ParseType(type));
        }

        public static System.Object GetComponent(Entity entity, System.Type type) {
            if (entity == null) {
                return null;
            }
            if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
                return null;
            }
            if (entityList.TryGetValue(type, out var cref)) {
                return cref.Get();
            }
            foreach (var cr in entityList) {
                if (type.IsAssignableFrom(cr.Key)) {
                    return cr.Value.Get();
                }
            }
            return null;
        }

        public static T Get<T>(this Entity entity) where T : IComponent {
            if (entity == null) {
                return default(T);
            }
            if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
                return default(T);
            }
            var type = typeof(T);
            return entityList.TryGetValue(type, out var cref) ? ((ManagedArray<T>) cref.Array)[cref.Index] : default(T);
        }

        public static T GetOrAdd<T>(this Entity entity) where T : IComponent, new() {
            if (entity == null) {
                return default(T);
            }
            if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
                var component = new T();
                entity.Add(component);
                return component;
            }
            var type = typeof(T);
            if (entityList.TryGetValue(type, out var cref)) {
                return ((ManagedArray<T>) cref.Array)[cref.Index];
            }
            return entity.Add(new T());
        }

        public static List<T> GetAllMatching<T>(this Entity entity) where T : IComponent {
            if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
                return null;
            }
            var targetType = typeof(T);
            List<T> returnList = new List<T>();
            foreach (var cref in entityList) {
                var compareType = cref.Key;
                if (!targetType.IsAssignableFrom(compareType) || !compareType.IsClass) {
                    continue;
                }
                returnList.Add((T) cref.Value.Get());
            }
            return returnList.Count > 0 ? returnList : null;
        }

        public static bool Get<T>(this Entity entity, Action<T> del) where T : IComponent {
            if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
                return false;
            }
            var type = typeof(T);
            if (!entityList.TryGetValue(type, out var cref)) {
                return false;
            }
            del(((ManagedArray<T>) cref.Array)[cref.Index]);
            return true;
        }

        public static void Remove<T>(this Entity entity, T component) where T : IComponent {
            entity.Remove(typeof(T));
        }

        public static void Remove<T>(this Entity entity) where T : IComponent {
            entity.Remove(typeof(T));
        }

        public static void Remove(this Entity entity, System.Type type){
            if (!_entityComponents.TryGetValue(entity.Id, out var entityComponentList)) {
                return;
            }
            if (!entityComponentList.TryGetValue(type, out var cref)) {
                return;
            }
            Remove(entity, cref, type);
            entityComponentList.Remove(type);
            if (_filtersToCheck.TryGetValue(type, out var filterList)) {
                for (int f = 0; f < filterList.Count; f++) {
                    filterList[f].CheckRemove(entity, entityComponentList);
                }
            }
        }

        private static void Remove(Entity entity, ComponentReference reference, System.Type type) {
            if (!_components.TryGetValue(type, out var componentList)) {
                return;
            }
            if (!(componentList.Get(reference.Index) is IComponent component)) {
                return;
            }
            componentList.Remove(reference.Index);
            if (component is IReceive receive) {
                entity.RemoveObserver(receive);
            }
            if (component is IDisposable dispose) {
                dispose.Dispose();
            }
            component.Owner = -1;
        }

        public static Entity GetParentOrSelf(this Entity entity) {
            if (entity.ParentId < 0) {
                return entity;
            }
            var parent = GetEntity(entity.ParentId);
            return parent ?? entity;
        }

        public static Entity GetParent(this Entity entity) {
            return entity.ParentId < 0 ? null : GetEntity(entity.ParentId);
        }

        public static void TryRoot(this Entity entity, Action<Entity> del) {
            var parent = entity.GetRoot();
            if (parent != null) {
                del(parent);
            }
        }

        public static void TryParent(this Entity entity, Action<Entity> del) {
            var parent = entity.GetParent();
            if (parent != null) {
                del(parent);
            }
        }

        public static Entity GetRoot(this IComponent component) {
            if (component == null) {
                return null;
            }
            return GetRoot(GetEntity(component.Owner));
        }

        private static WhileLoopLimiter _loopLimiter = new WhileLoopLimiter(5000);

        public static Entity GetRoot(this Entity entity) {
            if (entity == null) {
                return null;
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
                var pc = checkEntity.Get<PositionComponent>();
                if (pc != null) {
                    return pc.Position;
                }
                if (checkEntity.Tr != null) {
                    return checkEntity.Tr.position;
                }
                checkEntity = checkEntity.GetParent();
            }
            return Vector3.zero;
        }

        public static Quaternion GetRotation(this Entity entity) {
            if (entity == null) {
                return Quaternion.identity;
            }
            return entity.Find<RotationComponent>()?.Rotation ?? Quaternion.identity;
        }

        public static void Spawn(this Entity entity, out Vector3 spawnPos, out Quaternion spawnRot) {
            var spawnTr = entity.GetSelfOrParent<AnimTr>().Tr;
            if (spawnTr != null) {
                spawnPos = spawnTr.position;
                spawnRot = spawnTr.rotation;
            }
            else {
                spawnPos = entity.GetPosition();
                spawnRot = entity.GetRotation();
            }
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
            return entity.Tags.Contain(EntityTags.IsDead);
        }

        public static T FindStat<T>(this Entity entity, string statFullID) where T : BaseStat {
            WhileLoopLimiter.ResetInstance();
            var currentEntity = entity;
            while (WhileLoopLimiter.InstanceAdvance()) {
                if (currentEntity == null) {
                    return null;
                }
                var stat = currentEntity.Stats.Get<T>(statFullID);
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
                var stat = currentEntity.Stats.Get(statFullID);
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
                if (currentEntity.Stats.HasStat(statFullID)) {
                    var stat = currentEntity.Stats.Get<T>(statFullID);
                    if (stat != null) {
                        del(stat);
                        return true;
                    }
                }
                currentEntity = currentEntity.GetParent();
            }
            return false;
        }

        public static void FindSpawn(this Entity entity, out Vector3 spawnPos, out Quaternion spawnRot) {
            var spawnTr = entity.Find<AnimTr>().Tr;
            var target = entity.Find<CommandTarget>();
            if (spawnTr != null) {
                spawnPos = spawnTr.position;
                spawnRot = spawnTr.rotation;
            }
            else if (target != null) {
                spawnPos = target.GetPosition;
                spawnRot = target.GetRotation;
            }
            else {
                spawnPos = entity.GetPosition();
                spawnRot = entity.GetRotation();
            }
        }

        //public static List<T> GetAll<T>(this Entity entity) where T : IComponent {
        //    var type = typeof(T);
        //    if (!_components.TryGetValue(type, out var list)) {
        //        return null;
        //    }
        //    if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
        //        return null;
        //    }
        //    List<T> returnList = new List<T>();
        //    for (int i = 0; i < entityList.Count; i++) {
        //        if (entityList[i].Type == type) {
        //            returnList.Add(((ManagedArray<T>)list)[entityList[i].Index]);
        //        }
        //    }
        //    return returnList.Count > 0 ? returnList : null;
        //}

        //public static List<T> DestructiveRetrieve<T>() where T : IEntityMessage {
        //    if (!_entityMessageList.TryGetValue(typeof(T).GetHashCode(), out var msgList) || msgList.Count == 0) {
        //        return null;
        //    }
        //    List<T> targetList = new List<T>();
        //    for (int i = 0; i < msgList.Count; i++) {
        //        targetList.Add((T) msgList[i]);
        //    }
        //    msgList.Clear();
        //    return targetList;
        //}

        //public static float GatherFloat<T>(this IEntity entity, int messageEvent, T msg) where T : IEntityMessage {
        //    if (!_componentDict.TryGetValue(entity, out var list)) {
        //        return 0;
        //    }
        //    float val = 0;
        //    for (int i = 0; i < list.Count; i++) {
        //        if (list[i] is IProvideFloat<T> gatherFloater) {
        //            val += gatherFloater.GatherFloat(messageEvent, msg);
        //        }
        //    }
        //    return val;
        //}
    }
}
