#define CONTRACT_DEBUG
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PixelComrades {
    public class JsonContractResolver : DefaultContractResolver {

        private static Dictionary<Type, JsonConverter> converters = new Dictionary<Type, JsonConverter>();

        private static T GetConverter<T>() where T : JsonConverter, new() {
            JsonConverter conv;

            if (converters.TryGetValue(typeof(T), out conv)) {
                return (T)conv;
            }

            conv = new T();

            converters.Add(typeof(T), conv);

            return (T)conv;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
            var property = base.CreateProperty(member, memberSerialization);
            property.ShouldSerialize = instance => {
#if !CONTRACT_DEBUG
                try {
#endif
                    if (ReflectionHelper.HasIgnoredAttribute(member)) {
                        return false;
                    }
                    if (member is PropertyInfo) {
                        var prop = (PropertyInfo)member;

                        if (prop.CanRead && prop.CanWrite && !prop.IsSpecialName) {
                            prop.GetValue(instance, null);
                            return true;
                        }
                    }
                    else if (member is FieldInfo) {
                        return true;
                    }
#if !CONTRACT_DEBUG
                }
                catch (Exception e) {
                    Debug.LogWarning("Can't create field \"" + member.Name + "\" " + member.DeclaringType + " -> " + member.ReflectedType + "\n\n" + e.ToString());
                }
#endif
                return false;
            };
            return property;
        }
        
        protected override JsonConverter ResolveContractConverter(Type type) {
            if (typeof(Color).IsAssignableFrom(type) || typeof(Color32).IsAssignableFrom(type)) {
                return GetConverter<JsonColorConverter>();
            }
            if (typeof(Matrix4x4).IsAssignableFrom(type)) {
                return GetConverter<JsonMatrixConverter>();
            }
            if (typeof(Vector2).IsAssignableFrom(type) ||
                typeof(Vector3).IsAssignableFrom(type) ||
                typeof(Vector4).IsAssignableFrom(type) ||
                typeof(Quaternion).IsAssignableFrom(type)) {
                return GetConverter<JsonVectorConverter>();
            }
            //if (typeof(Material).IsAssignableFrom(type)) {
            //    return GetConverter<JsonMaterialConverter>();
            //}
            //if (typeof(ScriptableObject).IsAssignableFrom(type)) {
            //    return GetConverter<JsonScriptableObjectConverter>();
            //}
            return base.ResolveContractConverter(type);
        }
    }
}
