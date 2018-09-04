using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PixelComrades {
    public static class EcsDebug  {
        public static void RegisterDebugCommands() {
            DebugLogConsole.AddCommandStatic("PostSignal", "PostSignal", typeof(EcsDebug));
            DebugLogConsole.AddCommandStatic("ListEntities", "ListEntities", typeof(EcsDebug));
            DebugLogConsole.AddCommandStatic("ListComponents", "ListComponents", typeof(EcsDebug));
            DebugLogConsole.AddCommandStatic("DebugComponent", "DebugComponent", typeof(EcsDebug));
        }

        public static void PostSignal(int entity, int message) {
            EntityController.GetEntity(entity)?.Post(message);
        }

        public static void ListEntities() {
            StringBuilder sb = new StringBuilder();
            EntityController.EntitiesArray.RunAction(e => {
                    var label = e.Get<LabelComponent>()?.Text ?? "None";
                    sb.Append(e.Id);
                    sb.Append(" ");
                    sb.AppendNewLine(label);
                }
            );
            DebugLogManager.Log("entities", sb.ToString(), LogType.Log);
        }

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
            DebugLogManager.Log(id + " components", sb.ToString(), LogType.Log);
        }

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
            DebugLogManager.Log(id + " components", sb.ToString(), LogType.Log);
        }
        
    }
}
