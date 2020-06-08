using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        [Command("listCharacters")]
        public static void ListCharacters() {
            var characterNodes = EntityController.GetTemplateList<CharacterTemplate>();
            Console.Log("Character Count " + characterNodes.UsedCount);
            characterNodes.Run(
                (ref CharacterTemplate node) => {
                    Console.Log("Character " + node.Entity.DebugId);
                });
        }

        [Command("writeDebugLog")]
        public static void WriteDebugLog() {
            StreamWriter writer = new StreamWriter(string.Format("{0}/DebugLog_{1:MM-dd-yy hh-mm-ss}.txt", Application.persistentDataPath, System.DateTime
            .Now), 
            false);
            writer.Write(DebugLog.Current);
            writer.Close();
        }

        [Command("godMode")]
        public static void GodMode() {
            bool added = false;
            for (int i = 0; i < PlayerPartySystem.Party.Length; i++) {
                if (PlayerPartySystem.Party[i].Entity.HasComponent<BlockDamageFlat>()) {
                    PlayerPartySystem.Party[i].Entity.Remove<BlockDamageFlat>();
                }
                else {
                    added = true;
                    PlayerPartySystem.Party[i].Entity.Add(new BlockDamageFlat());
                }
            }
            Console.Log(added ? "Enabled god mode" : "Disabled god mode");
        }

        [Command("recoverEntity")]
        public static void RecoverEntity(int entityId, int amount) {
            var entity = EntityController.GetEntity(entityId);
            if (entity == null) {
                Console.Log("No Entity " + entityId);
                return;
            }
            World.Get<RulesSystem>().Post(new HealingEvent(amount, null, null, "Vitals.Energy"));
            Console.Log(entity.Get<StatsContainer>().GetVital("Vitals.Energy").ToLabelString());
        }
        

        [Command("healEntity")]
        public static void HealEntity(int entityId, int amount) {
            var entity = EntityController.GetEntity(entityId);
            if (entity == null) {
                Console.Log("No Entity " + entityId);
                return;
            }
            World.Get<RulesSystem>().Post(new HealingEvent(amount, null, null, "Vitals.Health"));
            Console.Log(entity.Get<StatsContainer>().GetVital("Vitals.Health").ToLabelString());
        }

        public static bool GodModeDamage(TakeDamageEvent dmg) {
            return true;
        }

        [Command("saveEntity")]
        public static void SaveEntity(int entityID) {
            var entity = EntityController.GetEntity(entityID);
            if (entity != null) {
                SerializingUtility.SaveJson(new SerializedEntity(entity), string.Format("{0}/{1}.json", Application
                .persistentDataPath, entity.DebugId));
                Console.Log($"Saved {entity.DebugId}");
            }
            
            Console.Log("Didn't provide a valid Entity ID");
        }

        [Command("timescale")]
        public static void ChangeTimeScale(float scale) {
            TimeManager.TimeScale = scale;
            Console.Log("Timescale is " + TimeManager.TimeScale.ToString("F2"));
        }

        [Command("importGameData")]
        public static void ImportGameData() {
            GameData.Init();
        }

        [Command("runMessageTest")]
        public static void RunMessageTest(int testCount) {
            var entity = Player.MainEntity;
            for (int i = 0; i < testCount; i++) {
                entity.Post(new MoveTweenEvent(null, null, null));
            }
            Console.Log("Finished Test");
        }

        [Command("runActionDelTest")]
        public static void RunActionDelTest(int testCount) {
            ManagedArray<Entity> array = EntityController.EntitiesArray;
            _stored = TestEntity;
            RunImplicitActionTest(array, testCount);
            RunStoredActionTest(array, testCount);
            RunExplicitActionTest(array, testCount);
        }

        public static ManagedArray<Entity>.RefDelegate _stored;

        public static void RunImplicitActionTest(ManagedArray<Entity> array, int testCount) {
            for (int i = 0; i < testCount; i++) {
                array.Run(TestEntity);
            }
        }

        public static void RunExplicitActionTest(ManagedArray<Entity> array, int testCount) {
            ManagedArray<Entity>.RefDelegate del = TestEntity;
            for (int i = 0; i < testCount; i++) {
                array.Run(del);
            }
        }

        public static void RunStoredActionTest(ManagedArray<Entity> array, int testCount) {
            for (int i = 0; i < testCount; i++) {
                array.Run(_stored);
            }
        }

        public static void TestEntity(ref Entity entity) {}

        [Command("version")]
        public static void Version() {
            Debug.LogFormat("Game Version: {0}", Game.Version);
        }

        [Command("testTimers")]
        public static void TestTimers() {
            TimeManager.StartUnscaled(RunTimerTest(1));
        }

        [Command("debugStatus")]
        public static void DebugStatus(int entity) {
            DebugStatus(EntityController.GetEntity(entity));
        }

        public static void DebugStatus(Entity entity) {
            var status = entity.Get<StatusUpdateComponent>();
            Debug.Log(status?.Status ?? "No Status Component");
        }
        //
        // public static void DebugVelocity(Entity entity) {
        //     var rb = entity.Get<RigidbodyComponent>().Rb;
        //     if (rb != null) {
        //         UIGenericValueWatcher.Get(UIAnchor.TopLeft, 0.25f, () => string.Format("Velocity: {0:F1} / {1:F1}",  
        //             rb.velocity.magnitude, entity.Get<StatsContainer>().GetValue("Attributes.Speed")));
        //     }
        // }

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

        [Command("flyCam")]
        public static void FlyCam() {
            PixelComrades.FlyCam.main.ToggleActive();
        }

        [Command("screenShot")]
        public static void ScreenShot() {
            ScreenCapture.CaptureScreenshot(
                string.Format("Screenshots/{0}-{1:MM-dd-yy hh-mm-ss}.png", Game.Title, System.DateTime.Now));
        }
        
        public static void FPS() {
            UIFrameCounter.main.Toggle();
        }

        [Command("fixMouse")]
        public static void FixMouse() {
            if (GameOptions.MouseLook && !Game.CursorUnlocked) {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (GameOptions.MouseLook) {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        [Command("debugMouseLock")]
        public static void DebugMouseLock() {
            Debug.LogFormat("MouseUnlocked {0}", Game.CursorUnlockedHolder.Debug());
        }

        [Command("debugPause")]        
        public static void DebugPause() {
            Debug.LogFormat("DebugPause {0}", Game.PauseHolder.Debug());
        }

        [Command("debugMenus")]
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

        [Command("addItem")]
        public static void AddItem(string template) {
            World.Get<ContainerSystem>().TryAdd(Player.MainInventory, ItemFactory.GetItem(template));
        }

        [Command("listUpdaters")]        
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

        [Command("listTurnUpdaters")]
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

        [Command("listEntities")]
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

        [Command("listComponents")]
        public static void ListComponents(int id) {
            var dict = EntityController.GetEntity(id).GetAllComponents();
            if (dict == null) {
                Debug.LogFormat("{0} has no components", id);
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(EntityController.GetEntity(id).Name);
            sb.AppendNewLine(" Components");
            foreach (var cRef in dict) {
                sb.AppendNewLine(string.Format("Type {0} Index {1}", cRef.Array.ArrayType.Name, cRef.Index));
            }
            
            Debug.Log(sb.ToString());
        }

        [Command("listEntityContainer")]
        public static void ListEntityContainer(int id, string typeName) {
            var dict = EntityController.GetEntity(id).GetAllComponents();
            if (dict == null) {
                Debug.LogFormat("{0} has no components", id);
                return;
            }
            System.Type type = null;
            ItemInventory instance = null;
            foreach (var cRef in dict) {
                if (cRef.Array.ArrayType.Name == typeName) {
                    type = cRef.Array.ArrayType;
                    instance = cRef.Get<ItemInventory>();
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

        [Command("debugComponent")]
        public static void DebugComponent(int id, string typeName) {
            var dict = EntityController.GetEntity(id).GetAllComponents();
            if (dict == null) {
                Debug.LogFormat("{0} has no components", id);
                return;
            }
            System.Type type = null;
            System.Object instance = null;
            foreach (var cRef in dict) {
                if (cRef.Array.ArrayType.Name == typeName) {
                    type = cRef.Array.ArrayType;
                    instance = cRef.Get();
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

        [Command("testSerializeComponent")]
        public static void TestSerializeComponent(int id, string typeName) {
            var dict = EntityController.GetEntity(id).GetAllComponents();
            if (dict == null) {
                Debug.LogFormat("{0} has no components", id);
                return;
            }
            System.Type type = null;
            IComponent instance = null;
            foreach (var cRef in dict) {
                if (cRef.Array.ArrayType.Name == typeName) {
                    type = cRef.Array.ArrayType;
                    instance = cRef.Get() as IComponent;
                }
            }
            if (type == null || instance == null) {
                Debug.LogFormat("{0} doesn't have component {1}", id, typeName);
                return;
            }
            TestSerializeComponent(instance);
        }

        private static void TestSerializeComponent(IComponent instance) {
            var json = JsonConvert.SerializeObject(instance, Formatting.Indented, Serializer.ConverterSettings);
            if (json != null) {
                Debug.Log(json);
            }
        }
    }
}
