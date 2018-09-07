using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using PixelComrades.DungeonCrawler;
using SeawispHunter.MinibufferConsole;

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
            Minibuffer.Register(typeof(EcsDebug));
        }

        [SeawispHunter.MinibufferConsole.Command]
        public static void Version() {
            Debug.LogFormat("Game Version: {0}", Game.Version);
        }

        [SeawispHunter.MinibufferConsole.Command]
        public static void TestTimers() {
            TimeManager.StartUnscaled(RunTimerTest(1));
        }

        [SeawispHunter.MinibufferConsole.Command]
        public static void TestAnimationEvent(int entityId, string clip) {
            var entity = EntityController.GetEntity(entityId);
            if (entity == null) {
                return;
            }
            var handle = new TestReceiver<AnimatorEvent>();
            entity.AddObserver(handle);
            entity.Post(new PlayAnimation(entity, clip, new AnimatorEvent(entity, clip, true, true)));
            handle.OnDel += receiver => entity.RemoveObserver(handle);
        }



        //[SeawispHunter.MinibufferConsole.Command]
        //public static void SetVital(int entity, int vital, float amount) {
        //    entity.
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

        [SeawispHunter.MinibufferConsole.Command]
        public static void FlyCam() {
            PixelComrades.FlyCam.main.ToggleActive();
        }

        [SeawispHunter.MinibufferConsole.Command]
        public static void Screenshot() {
            ScreenCapture.CaptureScreenshot(
                string.Format( "Screenshots/{0}-{1:MM-dd-yy hh-mm-ss}.png", Game.Title, System.DateTime.Now));
        }

        [SeawispHunter.MinibufferConsole.Command]
        public static void FPS() {
            UIFrameCounter.main.Toggle();
        }

        [SeawispHunter.MinibufferConsole.Command]
        public static void FixMouse() {
            if (GameOptions.MouseLook && !Game.CursorUnlocked) {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (GameOptions.MouseLook) {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        [SeawispHunter.MinibufferConsole.Command]
        public static void DebugMouseLock() {
            Debug.LogFormat("MouseUnlocked {0}", Game.CursorUnlockedHolder.Debug());
        }

        [SeawispHunter.MinibufferConsole.Command]
        public static void DebugPause() {
            Debug.LogFormat("DebugPause {0}", Game.PauseHolder.Debug());
        }

        [SeawispHunter.MinibufferConsole.Command]
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

        [SeawispHunter.MinibufferConsole.Command]
        public static void PostSignal(int entity, int message) {
            EntityController.GetEntity(entity)?.Post(message);
        }

        [SeawispHunter.MinibufferConsole.Command]
        public static void AddItem(string template) {
            Player.MainInventory.Add(ItemDatabase.GetItem(template));
        }

        [SeawispHunter.MinibufferConsole.Command]
        public static void AddRandomItem() {
            Player.MainInventory.Add(ItemDatabase.Instance.TestItemCreate());
        }

        [SeawispHunter.MinibufferConsole.Command]
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

        [SeawispHunter.MinibufferConsole.Command]
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

        [SeawispHunter.MinibufferConsole.Command]
        public static void ListEntities() {
            StringBuilder sb = new StringBuilder();
            EntityController.EntitiesArray.RunAction(e => {
                    var label = e.Get<LabelComponent>()?.Text ?? "None";
                    sb.Append(e.Id);
                    sb.Append(" ");
                    sb.AppendNewLine(label);
                }
            );
            Debug.LogFormat("entities {0}", sb.ToString());
        }

        [SeawispHunter.MinibufferConsole.Command]
        public static void ListComponents(int id) {
            var dict = EntityController.GetEntityComponentDict(id);
            if (dict == null) {
                Debug.LogFormat("{0} has no components", id);
                return;
            }
            StringBuilder sb = new StringBuilder();
            foreach (var cRef in dict) {
                sb.AppendNewLine(string.Format("Type {0} Index {1}", cRef.Key, cRef.Value.Index));
            }
            Debug.LogFormat("{0} components {1}", id, sb.ToString());
        }

        [SeawispHunter.MinibufferConsole.Command]
        public static void ListEntityContainer(int id, string typeName) {
            var dict = EntityController.GetEntityComponentDict(id);
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

        [SeawispHunter.MinibufferConsole.Command]
        public static void DebugComponent(int id, string typeName) {
            var dict = EntityController.GetEntityComponentDict(id);
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

        [SeawispHunter.MinibufferConsole.Command]
        public static void TestSerializeComponent(int id, string typeName) {
            var dict = EntityController.GetEntityComponentDict(id);
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
            var json = JsonConvert.SerializeObject(instance, Formatting.Indented, Serializer.ConverterSettings);
            if (json != null) {
                Debug.Log(json);
            }
        }
    }
}
