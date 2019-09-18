using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace PixelComrades {

    public class TestReceiver<T>  : IReceive<T> where T : IEntityMessage {

        public System.Action<TestReceiver<T>> OnDel;

        public void Handle(T arg) {
            Debug.LogFormat("Received {0}", arg.GetType());
            OnDel.SafeInvoke(this);
        }
    }
    
    public static class EcsDebug {
        //DebugLogConsole.AddCommandStatic("debugStats", "Toggle", typeof(DebugText));
        //DebugLogConsole.AddCommandInstance("debugWorldControl", "ShowDebug", this);

        public static void RegisterDebugCommands() {
            DebugConsole.RegisterCommand("actionTest", RunActionDelTest);
            DebugConsole.RegisterCommand("messageTest", RunMessageTest);
            DebugConsole.RegisterCommand("timescale", ChangeTimeScale);
            DebugConsole.RegisterCommand("saveEntity", SaveEntity);
            DebugConsole.RegisterCommand("heal", strings => {
                Player.Party[0].Entity.Post(new HealEvent(100, null, null, "Vitals.Health"));
                return Player.Party[0].Stats.GetVital("Vitals.Health").ToLabelString();
            });
            DebugConsole.RegisterCommand("recover", strings => {
                Player.Party[0].Entity.Post(new HealEvent(100, null, null, "Vitals.Energy"));
                return Player.Party[0].Stats.GetVital("Vitals.Energy").ToLabelString();
            });
            DebugConsole.RegisterCommand("godmode", strings => {
                var block = Player.Party[0].Entity.GetOrAdd<BlockDamage>();
                if (block.Dels.Contains(GodModeDamage)) {
                    block.Dels.Remove(GodModeDamage);
                    return "Disabled god mode";
                }
                block.Dels.Add(GodModeDamage);
                return "Enabled god mode";
            });
            //Minibuffer.Register(typeof(EcsDebug));
        }

        private static bool GodModeDamage(DamageEvent dmg) {
            return true;
        }

        private static string SaveEntity(string[] entityID) {
            if (entityID == null) {
                return "Didn't provide an Entity ID'";
            }
            if (int.TryParse(entityID[0], out var result)) {
                var entity = EntityController.GetEntity(result);
                if (entity != null) {
                    SerializingUtility.SaveJson(new SerializedEntity(entity), string.Format("{0}/{1}.json", Application
                    .persistentDataPath, entity.DebugId));
                    return $"Saved {entity.DebugId}";
                }
            }
            return "Didn't provide a valid Entity ID";
        }
        

        private static string ChangeTimeScale(string[] scale) {
            if (scale == null) {
                TimeManager.TimeScale = 1;
                return "Timescale is 1";
            }
            if (float.TryParse(scale[0], out var result)) {
                TimeManager.TimeScale = result;
                return "Timescale is " + TimeManager.TimeScale.ToString("F2");
            }
            return "Didn't provide a float";
        }

        public static void ImportGameData() {
            GameData.Init();
        }

        private static string RunMessageTest(string[] count) {
            ManagedArray<Entity> array = EntityController.EntitiesArray;
            int testCount = 5000;
            if (count.Length > 0) {
                if (int.TryParse(count[0], out var convertedCount)) {
                    testCount = convertedCount;
                }
            }
            RunMessageTest(testCount);
            return "Finished Test";
        }

        private static void RunMessageTest(int testCount) {
            var entity = Player.MainEntity;
            for (int i = 0; i < testCount; i++) {
                entity.Post(new MoveTweenEvent(null, null, null));
            }
        }

        private static string RunActionDelTest(string[] count) {
            ManagedArray<Entity> array = EntityController.EntitiesArray;
            int testCount = 5000;
            if (count.Length > 0) {
                if (int.TryParse(count[0], out var convertedCount)) {
                    testCount = convertedCount;
                }
            }
            _stored = TestEntity;
            RunImplicitActionTest(array, testCount);
            RunStoredActionTest(array, testCount);
            RunExplicitActionTest(array, testCount);
            return "";
        }

        private static ManagedArray<Entity>.Delegate _stored;

        private static void RunImplicitActionTest(ManagedArray<Entity> array, int testCount) {
            for (int i = 0; i < testCount; i++) {
                array.Run(TestEntity);
            }
        }

        private static void RunExplicitActionTest(ManagedArray<Entity> array, int testCount) {
            ManagedArray<Entity>.Delegate del = TestEntity;
            for (int i = 0; i < testCount; i++) {
                array.Run(del);
            }
        }

        private static void RunStoredActionTest(ManagedArray<Entity> array, int testCount) {
            for (int i = 0; i < testCount; i++) {
                array.Run(_stored);
            }
        }

        private static void TestEntity(Entity entity) {}

        public static void Version() {
            Debug.LogFormat("Game Version: {0}", Game.Version);
        }

        public static void TestTimers() {
            TimeManager.StartUnscaled(RunTimerTest(1));
        }

        public static void DebugStatus(int entity) {
            DebugStatus(EntityController.GetEntity(entity));
        }

        public static void DebugStatus(Entity entity) {
            var status = entity.Get<StatusUpdateComponent>();
            Debug.Log(status?.Status ?? "No Status Component");
        }

        public static void DebugVelocity(Entity entity) {
            var rb = entity.Get<RigidbodyComponent>().Rb;
            if (rb != null) {
                UIGenericValueWatcher.Get(UIAnchor.TopLeft, 0.25f, () => string.Format("Velocity: {0:F1} / {1:F1}",  
                    rb.velocity.magnitude, entity.Get<StatsContainer>().GetValue("Attributes.Speed")));
            }
        }

        //public static void TestAnimationEvent(int entityId, string clip) {
        //    var entity = EntityController.GetEntity(entityId);
        //    if (entity == null) {
        //        return;
        //    }
        //    var handle = new TestReceiver<AnimatorEvent>();
        //    entity.AddObserver(handle);
        //    entity.Post(new PlayAnimation(entity, clip, new AnimatorEvent(entity, clip, true, true)));
        //    handle.OnDel += receiver => entity.RemoveObserver(handle);
        //}

        private static IEnumerator RunTimerTest(float length) {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            var timer = new UnscaledTimer(length);
            timer.StartTimer();
            var startTime = TimeManager.TimeUnscaled;
            while (timer.IsActive) {
                yield return null;
            }
            watch.Stop();
            Debug.LogFormat("Stop Watch Seconds {0} Ms {1} Manual {2} Timer {3}", watch.Elapsed.TotalSeconds, watch.Elapsed.Milliseconds, TimeManager.TimeUnscaled - startTime, length);
        }

        public static void FlyCam() {
            PixelComrades.FlyCam.main.ToggleActive();
        }
        
        public static void Screenshot() {
            ScreenCapture.CaptureScreenshot(
                string.Format( "Screenshots/{0}-{1:MM-dd-yy hh-mm-ss}.png", Game.Title, System.DateTime.Now));
        }
        
        public static void FPS() {
            UIFrameCounter.main.Toggle();
        }
        
        public static void FixMouse() {
            if (GameOptions.MouseLook && !Game.CursorUnlocked) {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (GameOptions.MouseLook) {
                Cursor.lockState = CursorLockMode.None;
            }
        }
        
        public static void DebugMouseLock() {
            Debug.LogFormat("MouseUnlocked {0}", Game.CursorUnlockedHolder.Debug());
        }

        
        public static void DebugPause() {
            Debug.LogFormat("DebugPause {0}", Game.PauseHolder.Debug());
        }

        
        public static void DebugMenus() {
            if (UIBasicMenu.OpenMenus.Count == 0) {
                Debug.Log("DebugMenus: 0");
            }
            else {
                System.Text.StringBuilder sb = new StringBuilder();
                for (int i = 0; i < UIBasicMenu.OpenMenus.Count; i++) {
                    sb.AppendNewLine(UIBasicMenu.OpenMenus[i].gameObject.name);
                }
                Debug.LogFormat("DebugMenus: {0}", UIBasicMenu.OpenMenus.Count);
            }
        }

        
        public static void PostSignal(int entity, int message) {
            EntityController.GetEntity(entity)?.Post(message);
        }

        
        public static void AddItem(string template) {
            Player.MainInventory.Add(ItemFactory.GetItem(template));
        }

        
        public static void ListUpdaters() {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < SystemManager.EveryUpdate.Count; i++) {
                var unityComponent = SystemManager.EveryUpdate[i] as UnityEngine.Component;
                sb.Append(i);
                sb.Append(": ");
                if (unityComponent != null) {
                    sb.AppendNewLine(unityComponent.name);
                }
                else {
                    sb.AppendNewLine(SystemManager.EveryUpdate[i].GetType().ToString());
                }
            }
            Debug.Log(sb.ToString());
        }

        
        public static void ListTurnUpdaters() {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < SystemManager.TurnUpdates.Count; i++) {
                var unityComponent = SystemManager.TurnUpdates[i] as UnityEngine.Component;
                sb.Append(i);
                sb.Append(": ");
                if (unityComponent != null) {
                    sb.AppendNewLine(unityComponent.name);
                }
                else {
                    sb.AppendNewLine(SystemManager.EveryUpdate[i].GetType().ToString());
                }
            }
            Debug.Log(sb.ToString());
        }

        
        public static void ListEntities() {
            StringBuilder sb = new StringBuilder();
            foreach (Entity e in EntityController.EntitiesArray) {
                var label = e.Get<LabelComponent>()?.Text ?? "None";
                sb.Append(e.Id);
                sb.Append(" ");
                sb.AppendNewLine(label);
            }
            Debug.LogFormat("entities {0}", sb.ToString());
        }

        
        public static void ListComponents(int id) {
            var dict = EntityController.GetEntity(id).Components;
            if (dict == null) {
                Debug.LogFormat("{0} has no components", id);
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(EntityController.GetEntity(id).Name);
            sb.AppendNewLine(" Components");
            foreach (var cRef in dict) {
                sb.AppendNewLine(string.Format("Type {0} Index {1}", cRef.Key, cRef.Value.Index));
            }
            
            Debug.Log(sb.ToString());
        }

        
        public static void ListEntityContainer(int id, string typeName) {
            var dict = EntityController.GetEntity(id).Components;
            if (dict == null) {
                Debug.LogFormat("{0} has no components", id);
                return;
            }
            System.Type type = null;
            EntityContainer instance = null;
            foreach (var cRef in dict) {
                if (cRef.Key.Name == typeName) {
                    type = cRef.Key;
                    instance = cRef.Value.Get() as EntityContainer;
                }
            }
            if (type == null || instance == null) {
                Debug.LogFormat("{0} doesn't have component {1}", id, typeName);
                return;
            }
            Debug.LogFormat("Container has {0}", instance.Count);
            for (int i = 0; i < instance.Count; i++) {
                Debug.LogFormat("{0}: {1}", i, instance[i].Get<LabelComponent>()?.Text ?? instance[i].Id.ToString());
            }
        }

        
        public static void DebugComponent(int id, string typeName) {
            var dict = EntityController.GetEntity(id).Components;
            if (dict == null) {
                Debug.LogFormat("{0} has no components", id);
                return;
            }
            System.Type type = null;
            System.Object instance = null;
            foreach (var cRef in dict) {
                if (cRef.Key.Name == typeName) {
                    type = cRef.Key;
                    instance = cRef.Value.Get();
                }
            }
            if (type == null) {
                Debug.LogFormat("{0} doesn't have component {1}", id, typeName);
                return;
            }
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var fieldValues = type.GetFields(bindingFlags) .Select(field => field.GetValue(instance)).ToList();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < fieldValues.Count; i++) {
                sb.AppendNewLine(fieldValues[i].ToString());
            }
            Debug.LogFormat("{0} {1}: {2}", id, typeName, sb.ToString());
        }

        
        public static void TestSerializeComponent(int id, string typeName) {
            var dict = EntityController.GetEntity(id).Components;
            if (dict == null) {
                Debug.LogFormat("{0} has no components", id);
                return;
            }
            System.Type type = null;
            IComponent instance = null;
            foreach (var cRef in dict) {
                if (cRef.Key.Name == typeName) {
                    type = cRef.Key;
                    instance = cRef.Value.Get() as IComponent;
                }
            }
            if (type == null || instance == null) {
                Debug.LogFormat("{0} doesn't have component {1}", id, typeName);
                return;
            }
            TestSerializeComponent(instance);
        }

        public static void TestSerializeComponent(IComponent instance) {
            var json = JsonConvert.SerializeObject(instance, Formatting.Indented, Serializer.ConverterSettings);
            if (json != null) {
                Debug.Log(json);
            }
        }
    }
}
