using System;
using System.Collections.Generic;

namespace PixelComrades {
    public class World : Singleton<World> {

        public static bool ApplicationIsQuitting { get;set; }
        public static bool ChangingScene { get; set; }
        public static bool IsSetup { get; private set; }
        public static bool IsQuittingOrChangingScene() {
            if (ApplicationIsQuitting) {
                return true;
            }
            if (ChangingScene) {
                return true;
            }
            return false;
        }

        private Dictionary<int, object> _data = new Dictionary<int, object>();
        private static Dictionary<Type, SystemBase> _systems = new Dictionary<Type, SystemBase>();
        private static List<System.Type> _systemTypes = new List<Type>();
        private static List<IMainSystemUpdate> _updaters = new List<IMainSystemUpdate>();
        private static Dictionary<Type, List<IReceive>> _globalReceivers = new Dictionary<Type, List<IReceive>>();
        //private static Queue<Type> _msgTypesAwaitingProcess = new Queue<Type>();
        private static List<Type>[] _typeEvents = new []{ new List<Type>(), new List<Type>()};
        private static int _typeListIdx = 0;
        private static List<Type> TypeList { get { return _typeEvents[_typeListIdx]; } }
        private static Dictionary<Type, TypedMessageQueue> _msgLists = new Dictionary<Type, TypedMessageQueue>();
        private static SortByPriorityClass _typeSorter = new SortByPriorityClass();
        private static SortByPriorityReceiver _receiverSorter = new SortByPriorityReceiver();

        public static void Setup() {
            IsSetup = true;
            if (Instance == null) {
                return;
            }
            NodeFilter<VisibleNode>.New(VisibleNode.GetTypes(), (entity, list) => new VisibleNode(entity, list));
            NodeFilter<CharacterNode>.New(CharacterNode.GetTypes(), (entity, list) => new CharacterNode(entity, list));
            Get<ActionFxSystem>();
            Get<AnimatorSystem>();
            Get<CameraSystem>();
            Get<CollisionCheckSystem>();
            Get<CommandSystem>();
            Get<CollisionEventSystem>();
            Get<ContainerSystem>();
            Get<DespawnEntitySystem>();
            Get<DistanceSystem>();
            Get<EntityModifierSystem>();
            Get<FactionSystem>();
            Get<ItemSceneSystem>();
            Get<MoverSystem>();
            Get<PhysicsMoverSystem>();
            Get<RadiusSystem>();
            Get<TurnBasedSystem>();
            Get<CharacterRectSystem>();
            Get<SensorSystem>();
            Get<EntityUIPoolSystem>();
            EcsDebug.RegisterDebugCommands();
            //TODO: default setup here
        }

        public static T Add<T>(Type type = null) where T : new() {
            var hash = type == null ? typeof(T).GetHashCode() : type.GetHashCode();
            if (Instance._data.TryGetValue(hash, out var o)) {
                InitializeObject(o);
                return (T) o;
            }
            var created = new T();
            InitializeObject(created);
            Instance._data.Add(hash, created);
            return created;
        }

        public static T AddSystem<T>() where T : SystemBase {
            var type = typeof(T);
            if (_systems.TryGetValue(type, out var system)) {
                return system as T;
            }
            system = Activator.CreateInstance<T>();
            _systems.Add(type, system);
            _systemTypes.Add(type);
            if (system is IMainSystemUpdate update) {
                _updaters.Add(update);
            }
            Add(system);
            return (T) system;
        }

        public static T Get<T>() where T : SystemBase {
            var type = typeof(T);
            if (type.IsAbstract) {
                return _systems.TryGetValue(FindSystemThatImplements(type), out var implementedSystem) ? implementedSystem as T : default(T);
            }
            return _systems.TryGetValue(type, out var system) ? system as T: AddSystem<T>();
        }

        public static System.Type FindSystemThatImplements(System.Type type) {
            for (int i = 0; i < _systemTypes.Count; i++) {
                if (type.IsAssignableFrom(_systemTypes[i])) {
                    return _systemTypes[i];
                }
            }
            return null;
        }

        public void ClearSessionData() {
            if (ApplicationIsQuitting) {
                return;
            }
            var toWipe = new List<int>();
            foreach (var pair in _data) {
                var needToBeWiped = pair.Value as IMustBeWipedOut;
                if (needToBeWiped != null) {
                    toWipe.Add(pair.Key);
                }
                var needToBeCleaned = pair.Value as IDisposable;
                if (needToBeCleaned == null) {
                    continue;
                }
                needToBeCleaned.Dispose();
            }
            for (var i = 0; i < toWipe.Count; i++) {
                _data.Remove(toWipe[i]);
            }
        }

        public static bool Contains<T>() {
            return Instance._data.ContainsKey(typeof(T).GetHashCode());
        }

        public static object Get(Type t) {
            object resolve;
            Instance._data.TryGetValue(t.GetHashCode(), out resolve);
            return resolve;
        }

        //public static T Get<T>() {
        //    var hasValue = Instance._data.TryGetValue(typeof(T).GetHashCode(), out var resolve);
        //    if (!hasValue) {
        //        Instance._data.TryGetValue(typeof(T).GetHashCode(), out resolve);
        //    }
        //    return (T) resolve;
        //}

        public static void InitializeObject(object obj) {
            var awakeble = obj as IAwake;
            if (awakeble != null) {
                awakeble.OnAwake();
            }
        }
        
        public static void Add(object obj) {
            var reciever = obj as IReceive;
            if (reciever == null) {
                return;
            }
            var all = obj.GetType().GetInterfaces();
            foreach (var intType in all) {
                if (intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IReceiveGlobal<>)) {
                    Instance.Add(reciever, intType.GetGenericArguments()[0]);
                }
                //else if (intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IReceiveRefGlobal<>)) {
                //    Instance.Add(reciever, intType.GetGenericArguments()[0]);
                //}
            }
        }

        private List<IReceive> GetList(Type type) {
            if (!_globalReceivers.TryGetValue(type, out var list)) {
                list = new List<IReceive>();
                _globalReceivers.Add(type, list);
            }
            return list;
        }

        public void Add<T>(IReceive receive) {
            var list = GetList(typeof(T));
            list.Add(receive);
            list.Sort(_receiverSorter);
        }

        public void Add(IReceive receive, Type type) {
            GetList(type).Add(receive);
        }
        
        public static void RemoveSystem(SystemBase system) {
            if (system is IMainSystemUpdate update) {
                _updaters.Remove(update);
            }
            _systems.Remove(system.GetType());
            Remove(system);
        }

        public static void Remove(object obj) {
            if (!(obj is IReceive reciever)) {
                return;
            }
            var all = obj.GetType().GetInterfaces();
            foreach (Type intType in all) {
                if (intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IReceiveGlobal<>)) {
                    Instance.Remove(reciever, intType.GetGenericArguments()[0]);
                }
                //else if (intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IReceiveRefGlobal<>)) {
                //    Instance.Remove(reciever, intType.GetGenericArguments()[0]);
                //}
            }
        }
        
        public void Remove<T>(IReceive receive) {
            GetList(typeof(T)).Remove(receive);
        }

        public void Remove(IReceive receive, Type type) {
            GetList(type).Remove(receive);
        }

        public static void Enqueue<T>(T message) where T : struct, IEntityMessage {
            TypedMessageQueue<T> queue;
            if (!_msgLists.ContainsKey(typeof(T))) {
                queue = new TypedMessageQueue<T>();
                _msgLists[typeof(T)] = queue;
            }
            else {
                queue = (TypedMessageQueue<T>) _msgLists[typeof(T)];
            }
            queue.Enqueue(message);
            var type = typeof(T);
            if (!TypeList.Contains(type)) {
                TypeList.Add(type);
            }
            //if (!_msgTypesAwaitingProcess.Contains(type)) {
            //    _msgTypesAwaitingProcess.Enqueue(type);
            //}
        }

        public static void Update(float dt) {
            //while (_msgTypesAwaitingProcess.Count > 0) {
            //    var type = _msgTypesAwaitingProcess.Dequeue();
            //    if (!_globalReceivers.TryGetValue(type, out var list)) {
            //        continue;
            //    }
            //    _msgLists[type].Process(list);
            //}
            for (int i = 0; i < _updaters.Count; i++) {
                _updaters[i].OnSystemUpdate(dt);
            }
            var list = TypeList;
            list.Sort(_typeSorter);
            _typeListIdx = (int) MathEx.WrapAround(_typeListIdx + 1, 0, _typeEvents.Length);
            for (int i = 0; i < list.Count; i++) {
                var type = list[i];
                if (!_globalReceivers.TryGetValue(type, out var receiverList)) {
                    continue;
                }
                _msgLists[type].Process(receiverList);
            }
            list.Clear();
        }

        private abstract class TypedMessageQueue {
            public abstract void Process(List<IReceive> list);
        }

        private class TypedMessageQueue<T> : TypedMessageQueue where T : struct, IEntityMessage {
            private List<T>[] _msgs;
            private int _current = 0;
            private List<T> List { get { return _msgs[_current]; } }

            public TypedMessageQueue() {
                _msgs = new[] {
                    new List<T>(), new List<T>()
                };
            }

            public void Enqueue(T message) {
                List.Add(message);
            }

            public override void Process(List<IReceive> list) {
                var msgList = List;
                _current = (int) MathEx.WrapAround(_current + 1, 0, _msgs.Length);
                for (int i = 0; i < list.Count; i++) {
                    (list[i] as IReceiveGlobal<T>)?.HandleGlobal(msgList);
                }
                msgList.Clear();
            }
        }
    }
}