using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;

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
        private static List<IMainSystemUpdate> _updates = new List<IMainSystemUpdate>();
        private static List<IMainFixedUpdate> _fixedUpdates = new List<IMainFixedUpdate>();
        private static List<IPeriodicUpdate> _periodicUpdates = new List<IPeriodicUpdate>();
        private static Dictionary<Type, List<IReceive>> _globalDelegateReceivers = new Dictionary<Type, List<IReceive>>();
        private static Dictionary<Type, List<IReceive>> _globalArrayReceivers = new Dictionary<Type, List<IReceive>>();
        //private static Queue<Type> _msgTypesAwaitingProcess = new Queue<Type>();
        private static List<Type>[] _typeEvents = new []{ new List<Type>(), new List<Type>()};
        private static int _typeListIdx = 0;
        private static UnscaledTimer _periodicTimer = new UnscaledTimer(0.5f);
        private static List<Type> TypeList { get { return _typeEvents[_typeListIdx]; } }
        private static Dictionary<Type, TypedMessageQueue> _msgLists = new Dictionary<Type, TypedMessageQueue>();
        private static SortByPriorityClass _typeSorter = new SortByPriorityClass();
        private static SortByPriorityReceiver _receiverSorter = new SortByPriorityReceiver();

        public static void Setup() {
            IsSetup = true;
            if (Instance == null) {
                return;
            }
            TemplateFilter<VisibleTemplate>.Setup(VisibleTemplate.GetTypes());
            TemplateFilter<CharacterTemplate>.Setup(CharacterTemplate.GetTypes());
            TemplateFilter<CollidableTemplate>.Setup(CollidableTemplate.GetTypes());
            Get<AnimatorSystem>();
            Get<CommandSystem>();
            Get<CollisionCheckSystem>();
            Get<DespawnEntitySystem>();
            Get<DistanceSystem>();
            Get<FactionSystem>();
            Get<ItemSceneSystem>();
            Get<ModifierSystem>();
            Get<MoverSystem>();
            Get<PhysicsMoverSystem>();
            Get<CharacterRectSystem>();
            Get<SensorSystem>();
            Get<EntityUIPoolSystem>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int a = 0; a < assemblies.Length; a++) {
                var types = assemblies[a].GetTypes();
                for (int t = 0; t < types.Length; t++) {
                    var type = types[t];
                    if (type.IsDefined(typeof(AutoRegisterAttribute), false)) {
                        CreateSystem(type);
                    }
                }
            }
        }

        public static T Add<T>(Type type = null) where T : new() {
            var hash = type == null ? typeof(T).GetHashCode() : type.GetHashCode();
            if (Instance._data.TryGetValue(hash, out var o)) {
                return (T) o;
            }
            var created = new T();
            //InitializeObject(created);
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
            CheckUpdates(system, true);
            RegisterReceivers(system);
            return (T) system;
        }

        private static void CreateSystem(System.Type type) {
            if (_systems.ContainsKey(type)) {
                return;
            }
            var system = (SystemBase) Activator.CreateInstance(type);
            _systems.Add(type, system);
            _systemTypes.Add(type);
            CheckUpdates(system, true);
            RegisterReceivers(system);
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

        public void DisposeSystems() {
            var systems = _systems.Values.ToArray();
            for (var i = 0; i < systems.Length; i++) {
                var system = systems[i];
                system.Dispose();
            }
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

        public static SystemBase GetType(string typeName) {
            var type = ParseUtilities.ParseType(typeName);
            if (type != null) {
                return _systems.TryGetValue(type, out var system) ? system : null;
            }
            return null;
        }

        //public static T Get<T>() {
        //    var hasValue = Instance._data.TryGetValue(typeof(T).GetHashCode(), out var resolve);
        //    if (!hasValue) {
        //        Instance._data.TryGetValue(typeof(T).GetHashCode(), out resolve);
        //    }
        //    return (T) resolve;
        //}

        
        public static void RegisterReceivers(object obj) {
            var receiver = obj as IReceive;
            if (receiver == null) {
                return;
            }
            var all = obj.GetType().GetInterfaces();
            foreach (var type in all) {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReceiveGlobalArray<>)) {
                    var list = GetArrayList(type.GetGenericArguments()[0]);
                    list.Add(receiver);
                    list.Sort(_receiverSorter);
                }
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReceiveGlobal<>)) {
                    var targetType = type.GetGenericArguments()[0];
                    if (!_msgLists.TryGetValue(targetType, out var queue)) {
                        var generic = typeof(TypedMessageQueue<>).MakeGenericType(targetType);
                        queue = (TypedMessageQueue) Activator.CreateInstance(generic);
                        _msgLists.Add(targetType, queue);
                    }
                    queue.AddReceiver(receiver);
                }
            }
        }

        public void Add<T>(IReceive receive) {
            if (receive is IReceiveGlobalArray<T>) {
                var list = GetArrayList(typeof(T));
                list.Add(receive);
                list.Sort(_receiverSorter);
            }
            else {
                var queue = GetMessageQueueGeneric<T>();
                if (queue != null) {
                    queue.AddReceiver(receive);
                }
            }
        }

        public static void RemoveSystem(SystemBase system) {
            CheckUpdates(system, false);
            _systems.Remove(system.GetType());
            Remove(system);
        }

        private static void CheckUpdates(SystemBase system, bool add) {
            if (system is IMainSystemUpdate update) {
                if (add) {
                    _updates.Add(update);
                }
                else {
                    _updates.Remove(update);
                }
            }
            if (system is IMainFixedUpdate fixedUpdate) {
                if (add) {
                    _fixedUpdates.Add(fixedUpdate);
                }
                else {
                    _fixedUpdates.Remove(fixedUpdate);
                }
            }
            if (system is IPeriodicUpdate periodicUpdate) {
                if (add) {
                    _periodicUpdates.Add(periodicUpdate);
                }
                else {
                    _periodicUpdates.Remove(periodicUpdate);
                }
            }
        }

        public static void Remove(object obj) {
            if (!(obj is IReceive receiver)) {
                return;
            }
            var all = obj.GetType().GetInterfaces();
            foreach (var type in all) {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReceiveGlobalArray<>)) {
                    var list = GetArrayList(type.GetGenericArguments()[0]);
                    list.Remove(receiver);
                    list.Sort(_receiverSorter);
                }
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReceiveGlobal<>)) {
                    var targetType = type.GetGenericArguments()[0];
                    if (_msgLists.TryGetValue(targetType, out var queue)) {
                        queue.RemoveReceiver(receiver);
                    }
                }
            }
        }
        
        public void Remove<T>(IReceive receive) {
            if (receive is IReceiveGlobalArray<T>) {
                var list = GetArrayList(typeof(T));
                list.Remove(receive);
                list.Sort(_receiverSorter);
            }
            else {
                var queue = GetMessageQueueGeneric<T>();
                if (queue != null) {
                    queue.RemoveReceiver(receive);
                }
            }
        }

        public static void Enqueue<T>(T message) where T : struct, IEntityMessage {
            var queue = GetMessageQueue<T>();
            queue.Enqueue(message);
            var type = typeof(T);
            if (!TypeList.Contains(type)) {
                TypeList.Add(type);
            }
            //if (!_msgTypesAwaitingProcess.Contains(type)) {
            //    _msgTypesAwaitingProcess.Enqueue(type);
            //}
        }

        private static TypedMessageQueue<T> GetMessageQueue<T>() where T : struct, IEntityMessage {
            var type = typeof(T);
            if (!_msgLists.TryGetValue(type, out var queue)) {
                queue = new TypedMessageQueue<T>();
                _msgLists.Add(type, queue);
            }
            return (TypedMessageQueue<T>) queue;
        }

        private static List<IReceive> GetArrayList(Type type) {
            if (!_globalArrayReceivers.TryGetValue(type, out var list)) {
                list = new List<IReceive>();
                _globalArrayReceivers.Add(type, list);
            }
            return list;
        }

        private static TypedMessageQueue GetMessageQueueGeneric<T>() {
            var type = typeof(T);
            if (!_msgLists.TryGetValue(type, out var queue)) {
                if (type.InheritsFrom(typeof(IEntityMessage)) && type.IsValueType) {
                    queue = new TypedMessageQueue<T>();
                    _msgLists.Add(type, queue);
                }
            }
            return queue;
        }

        public static void Update(float dt, float unscaledDt) {
            //while (_msgTypesAwaitingProcess.Count > 0) {
            //    var type = _msgTypesAwaitingProcess.Dequeue();
            //    if (!_globalReceivers.TryGetValue(type, out var list)) {
            //        continue;
            //    }
            //    _msgLists[type].Process(list);
            //}
            for (int i = 0; i < _updates.Count; i++) {
                _updates[i].OnSystemUpdate(dt, unscaledDt);
            }
            if (!_periodicTimer.IsActive) {
                _periodicTimer.StartTimer();
                for (int i = 0; i < _periodicUpdates.Count; i++) {
                    _periodicUpdates[i].OnPeriodicUpdate();
                }
            }
            var list = TypeList;
            list.Sort(_typeSorter);
            _typeListIdx = (int) MathEx.WrapAround(_typeListIdx + 1, 0, _typeEvents.Length);
            for (int i = 0; i < list.Count; i++) {
                var type = list[i];
                _globalArrayReceivers.TryGetValue(type, out var receiverList);
                _msgLists[type].Process(receiverList);
            }
            list.Clear();
        }

        public static void FixedUpdate(float dt) {
            for (int i = 0; i < _fixedUpdates.Count; i++) {
                _fixedUpdates[i].OnFixedSystemUpdate(dt);
            }
        }

        private abstract class TypedMessageQueue {
            public abstract void Process(List<IReceive> list);
            public abstract void AddReceiver(IReceive receiver);
            public abstract void RemoveReceiver(IReceive receiver);
        }

        private class TypedMessageQueue<T> : TypedMessageQueue {
            private BufferedList<T> _msgs = new BufferedList<T>();
            private List<ManagedArray<T>.Delegate> _globalDel = new List<ManagedArray<T>.Delegate>();
            private SortByPriorityClass<T> _sorter = new SortByPriorityClass<T>();

            public void Enqueue(T message) {
                _msgs.Add(message);
            }

            public override void AddReceiver(IReceive receiver) {
                if (receiver is IReceiveGlobal<T> del) {
                    _globalDel.Add(del.HandleGlobal);
                    _globalDel.Sort(_sorter);
                }
            }

            public override void RemoveReceiver(IReceive receiver) {
                if (receiver is IReceiveGlobal<T> del) {
                    _globalDel.Remove(del.HandleGlobal);
                    _globalDel.Sort(_sorter);
                }
            }

            public override void Process(List<IReceive> list) {
//                _msgs.Advance();
                if (list != null && _msgs.Count > 0) {
                    for (int i = 0; i < list.Count; i++) {
                        if (list[i] is IReceiveGlobalArray<T> globalDel) {
                            globalDel.HandleGlobal(_msgs);
                        }
                    }
                }
                for (int d = 0; d < _globalDel.Count; d++) {
                    var del = _globalDel[d];
                    _msgs.Run(del);
                    //_msgs.PreviousList.Run(_globalDel[i]);
                }
                _msgs.ClearCurrentAndDeletes();
            }
        }
    }
}