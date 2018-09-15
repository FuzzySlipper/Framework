using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PixelComrades {
    public class EntityFactory : SystemBase {

        private Dictionary<System.Type, IDataFactory> _factory = new Dictionary<Type, IDataFactory>();

        public EntityFactory() {
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

        public void Add(System.Type type, IDataFactory factory) {
            if (_factory.ContainsKey(type)) {
                _factory[type] = factory;
            }
            else {
                _factory.Add(type, factory);
            }
        }

        public void Remove(System.Type type) {
            _factory.Remove(type);
        }

        public void AddComponent(Entity entity, List<string> list, System.Type type) {
            if (!_factory.TryGetValue(type, out var factory)) {
                return;
            }
            factory.AddComponent(entity, list);
        }
    }
}
