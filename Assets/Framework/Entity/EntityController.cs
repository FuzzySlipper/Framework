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
        private static Dictionary<int, BufferedList<IReceive>> _entityMessageList = new Dictionary<int, BufferedList<IReceive>>();
        private static Dictionary<int, MessageKitLocal> _simpleEntityMessages = new Dictionary<int, MessageKitLocal>();
        private static Dictionary<Type, List<NodeFilter>> _filtersToCheck = new Dictionary<Type, List<NodeFilter>>();
        private static Dictionary<Type, NodeFilter> _filterHandler = new Dictionary<Type, NodeFilter>();
        private static SortByPriorityReceiver _msgSorter = new SortByPriorityReceiver();

        public static ManagedArray<Entity> EntitiesArray { get => _entities; }

        public static void Post<T>(this T msg, Entity entity) where T : struct, IEntityMessage {
            if (entity != null) {
                entity.ProcessEntityPost(msg);
            }
            World.Enqueue(msg);
        }

        public static void Post<T>(this Entity entity, T msg) where T : struct, IEntityMessage {
            entity.ProcessEntityPost<T>(msg);
            World.Enqueue(msg);
        }

        private static void ProcessEntityPost<T>(this Entity entity, T msg) where T : IEntityMessage {
            if (!_entityMessageList.TryGetValue(entity.Id, out var bufferedList)) {
                return;
            }
            bufferedList.Swap();
            var list = bufferedList.PreviousList;
            for (int i = 0; i < list.Count; i++) {
                (list[i] as IReceiveRef<T>)?.Handle(ref msg);
            }
            for (int i = 0; i < list.Count; i++) {
                (list[i] as IReceive<T>)?.Handle(msg);
            }
        }

        public static void Post(this Entity entity, int msg) {
            if (!_simpleEntityMessages.TryGetValue(entity.Id, out var list)) {
                return;
            }
            list.post(msg);
        }

        public static void AddObserver<T>(this Entity entity, IReceive<T> handler) {
            if (!_entityMessageList.TryGetValue(entity.Id, out var list)) {
                list = new BufferedList<IReceive>();
                _entityMessageList.Add(entity.Id, list);
            }
            if (list.CurrentList.Contains(handler)) {
                return;
            }
            list.CurrentList.Add(handler);
            list.CurrentList.Sort(_msgSorter);
        }

        public static void AddObserver(this Entity entity, IReceive handler) {
            if (!_entityMessageList.TryGetValue(entity.Id, out var list)) {
                list = new BufferedList<IReceive>();
                _entityMessageList.Add(entity.Id, list);
            }
            if (list.CurrentList.Contains(handler)) {
                return;
            }
            list.CurrentList.Add(handler);
            list.CurrentList.Sort(_msgSorter);
        }

        public static void AddObserver(this Entity entity, int message,  System.Action handler) {
            if (!_simpleEntityMessages.TryGetValue(entity.Id, out var list)) {
                list = new MessageKitLocal();
                _simpleEntityMessages.Add(entity.Id, list);
            }
            list.addObserver(message, handler);
        }

        public static void AddObserver(this Entity entity, ISignalReceiver generic) {
            if (!_simpleEntityMessages.TryGetValue(entity.Id, out var list)) {
                list = new MessageKitLocal();
                _simpleEntityMessages.Add(entity.Id, list);
            }
            list.addObserver(generic);
        }

        public static void RemoveObserver<T>(this Entity entity, IReceive<T> handler) {
            if (!_entityMessageList.TryGetValue(entity.Id, out var list)) {
                return;
            }
            list.Remove(handler);
        }

        public static void RemoveObserver(this Entity entity, IReceive handler) {
            if (!_entityMessageList.TryGetValue(entity.Id, out var list)) {
                return;
            }
            list.Remove(handler);
        }

        public static void RemoveObserver(this Entity entity, int message, System.Action handler) {
            if (!_simpleEntityMessages.TryGetValue(entity.Id, out var list)) {
                return;
            }
            list.removeObserver(message, handler);
        }

        public static void RemoveObserver(this Entity entity, ISignalReceiver generic) {
            if (!_simpleEntityMessages.TryGetValue(entity.Id, out var list)) {
                return;
            }
            list.removeObserver(generic);
        }

        public static void ClearAllMessages() {
            var enumerator = _entityMessageList.GetEnumerator();
            try {
                while (enumerator.MoveNext()) {
                    enumerator.Current.Value.Clear();
                }
            }
            finally {
                enumerator.Dispose();
            }
        }

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

        public static T GetNode<T>(this Entity entity) where T : class, INode {
            var type = typeof(T);
            return !_filterHandler.TryGetValue(type, out var filter) ? null : ((NodeFilter<T>) filter).GetNode(entity);
        }

        public static List<T> GetNodeList<T>() where T : class, INode {
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
            if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
                return null;
            }
            return entityList.TryGetValue(type, out var cRef) ? cRef : (ComponentReference?) null;
        }

        public static bool HasComponent<T>(this Entity entity) {
            var type = typeof(T);
            if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
                return false;
            }
            return entityList.ContainsKey(type);
        }

        public static bool HasDerivedComponent<T>(this Entity entity) {
            if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
                return false;
            }
            var type = typeof(T);
            return entityList.ContainsDerivedType(type);
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

        public static Entity AddEntity(Entity entity) {
            entity.Id = _entities.Add(entity);
            return entity;
        }

        public static void RemoveEntity(Entity entity) {
            entity.Post(new EntityDisposed(entity));
            _entities.Remove(entity.Id);
            if (_entityMessageList.TryGetValue(entity.Id, out var msgList)) {
                msgList.Clear();
                _entityMessageList.Remove(entity.Id);
            }
            MonoBehaviourToEntity.Unregister(entity);
            if (!_entityComponents.TryGetValue(entity, out var componentList)) {
                return;
            }
            foreach (var componentReference in componentList) {
                Remove(entity, componentReference.Value, componentReference.Key);
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
                if (((ManagedArray<T>) componentList)[index] is IReceive receiveOld) {
                    entity.RemoveObserver(receiveOld);
                }
                entityComponents[type] = new ComponentReference(index, componentList);
                ((ManagedArray<T>) componentList)[index] = newComponent;
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
            if (entity.HasComponent<T>() || entity.HasDerivedComponent<T>()) {
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
                if (parentList != null && parentList.ContainsDerivedType(type)) {
                    return parent.GetDerived<T>(del);
                }
                parent = parent.GetParent();
            }
            return false;
        }

        public static T Find<T>(this Entity entity) where T : IComponent {
            if (entity == null) {
                return default(T);
            }
            if (entity.HasComponent<T>() || entity.HasDerivedComponent<T>()) {
                return entity.Get<T>();
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
                    return parent.Get<T>();
                }
                if (parentList != null && parentList.ContainsDerivedType(type)) {
                    return parent.GetDerived<T>();
                }
                parent = parent.GetParent();
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
            if (entityList.ContainsDerivedType(type)) {
                return entity.GetDerived<T>();
            }
            return parent.Get<T>();
        }

        public static T Get<T>(this IComponent component) where T : IComponent {
            return GetEntity(component.Owner).Get<T>();
        }

        public static T Get<T>(this Entity entity) where T : IComponent {
            if (entity == null) {
                return default(T);
            }
            if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
                return default(T);
            }
            var type = typeof(T);
            if (entityList.TryGetValue(type, out var cref)) {
                return ((ManagedArray<T>) cref.Array)[cref.Index];
            }
            return entity.GetDerived<T>();
        }

        public static T GetDerived<T>(this Entity entity) where T : IComponent {
            if (entity == null) {
                return default(T);
            }
            if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
                return default(T);
            }
            var type = typeof(T);
            foreach (var cref in entityList) {
                if (type.IsAssignableFrom(cref.Key)) {
                    return (T) cref.Value.Get();
                }
            }
            return default(T);
        }

        public static bool GetDerived<T>(this Entity entity, Action<T> del) where T : IComponent {
            if (entity == null) {
                return false;
            }
            if (!_entityComponents.TryGetValue(entity.Id, out var entityList)) {
                return false;
            }
            var type = typeof(T);
            foreach (var cref in entityList) {
                if (type.IsAssignableFrom(cref.Key)) {
                    del((T) cref.Value.Get());
                }
            }
            return false;
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
            return entity.Find<PositionComponent>()?.Position ?? Vector3.zero;
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
            var stats = entity.Get<GenericStats>();
            return stats.GetValue(Stats.Speed);
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
            //else if (Owner.LastStateEvent != null) {
            //    spawnPos = Owner.LastStateEvent.Value.Position;
            //    spawnRot = Owner.LastStateEvent.Value.Rotation;
            //}
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
