using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PixelComrades {
    public class DataFactory : SystemBase {

        private Dictionary<System.Type, IDataFactory> _factories = new Dictionary<Type, IDataFactory>();
        private Dictionary<string, IDataFactory> _customFactories = new Dictionary<string, IDataFactory>();

        public DataFactory() {
            var type = typeof(IDataFactory<>);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => p.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == type))
                .ToArray();
            for (int t = 0; t < types.Length; t++) {
                var interfaces = types[t].GetInterfaces();
                for (var i = 0; i < interfaces.Length; i++) {
                    Type intType = interfaces[i];
                    if (intType.IsGenericType && intType.GetGenericTypeDefinition() == type) {
                        Add(intType.GetGenericArguments()[0], Activator.CreateInstance(types[t]) as IDataFactory);
                    }
                }
            }
        }

        public void AddComponentList(Entity entity, DataEntry data, DataList componentList) {
            if (componentList == null || componentList.Value.Count == 0) {
                return;
            }
            for (int i = 0; i < componentList.Value.Count; i++) {
                var type = ParseUtilities.ParseType(componentList.Value[i].GetValue<string>(DatabaseFields.Component));
                if (type == null) {
                    continue;
                }
                AddComponent(entity, data, type);
            }
        }

        public void Add(System.Type type, IDataFactory factory) {
            _factories.AddOrUpdate(type, factory);
        }

        public void Add(string type, IDataFactory factory) {
            _customFactories.AddOrUpdate(type, factory);
        }

        public void Remove(System.Type type) {
            _factories.Remove(type);
        }

        public void Remove(string type) {
            _customFactories.Remove(type);
        }

        public void AddComponent(Entity entity, DataEntry data, System.Type type) {
            if (!_factories.TryGetValue(type, out var factory)) {
                return;
            }
            factory.AddComponent(entity, data);
        }

        public void AddComponent(Entity entity, DataEntry data, string type) {
            if (!_customFactories.TryGetValue(type, out var factory)) {
                return;
            }
            factory.AddComponent(entity, data);
        }
    }
}
