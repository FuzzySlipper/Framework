using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace PixelComrades {
    public static class CastTo<T> {
        /// <summary>
        /// Casts <see cref="S"/> to <see cref="T"/>.
        /// This does not cause boxing for value types.
        /// Useful in generic methods.
        /// </summary>
        /// <typeparam name="S">Source type to cast from. Usually a generic type.</typeparam>
        public static T From<S>(S s) {
            return Cache<S>.caster(s);
        }

        private static class Cache<S> {
            public static readonly Func<S, T> caster = Get();

            private static Func<S, T> Get() {
                return Delegate.CreateDelegate(typeof(Func<S, T>), ((Func<T, T>) (x => x)).Method) as Func<S, T>;
            }
        }
    }

    public static class ReflectionHelper {

        //public static void BuildFromTextList(List<string> list) {
        //    for (int i = 0; i < list.Count; i++) {
        //        var className = "PixelComrades." + list[i];
        //        Type newPartyType = Type.GetType(className);
        //        if (newPartyType == null) {
        //            Debug.LogErrorFormat("{0} is invalid class", list[i]);
        //            continue;
        //        }
        //        object newPart = Activator.CreateInstance(newPartyType);
        //        // ((Ipart)NewPart.ParentObject = NewObject
        //        foreach (FieldInfo field in newPartyType.GetFields()) {
        //            try {
        //                //if parameters containskey (field.name)
        //            }
        //            catch (Exception e) {
        //                Debug.Log(e);
        //                throw;
        //            }
        //        }
        //    }
        //}

        public static bool Debugging = !Application.isPlaying;

        public static readonly HashSet<Type> IgnoreAttributes = new HashSet<Type> {
            typeof(ObsoleteAttribute),
            typeof(IgnoreFileSerialization)
        };

        private static Dictionary<string, string> _unityInternalMapped = new Dictionary<string, string>() {
            { "m_Materials", "sharedMaterials"}
        };

        public static string ScrubUnityInternal(string field) {
            foreach (var internals in _unityInternalMapped) {
                if (field.Contains(internals.Key)) {
                    field = field.Replace(internals.Key, internals.Value);
                }
            }
            return field;
        }

        ///<summary>
        ///  Returns name of any method expression with any number of parameters either void or with a return value
        ///</summary>
        ///<param name = "expression">
        ///  Any method expression with any number of parameters either void or with a return value
        ///</param>
        ///<returns>
        ///  Name of any method with any number of parameters either void or with a return value
        ///</returns>
        public static string Method(Expression<System.Action> expression) {
            if (expression == null) {
                return "Error";
            }
            return ((MethodCallExpression) expression.Body).Method.Name;
        }

        ///<summary>
        ///  Returns name of property, field or parameter expression (of anything but method)
        ///</summary>
        ///<param name = "expression">
        ///  Property, field or parameter expression
        ///</param>
        ///<returns>
        ///  Name of property, field, parameter
        ///</returns>
        public static string Member(Expression<Func<object>> expression) {
            if (expression == null) {
                return "Error";
            }
            if (expression.Body is UnaryExpression) {
                return ((MemberExpression) ((UnaryExpression) expression.Body).Operand).Member.Name;
            }
            return ((MemberExpression) expression.Body).Member.Name;
        }

        /**
         * Iterate through an object instance applying matching property keys to the object's fields.
         */
        public static void ApplyProperties<T>(T obj, Dictionary<string, object> properties) {
            foreach (var kvp in properties) {
                SetValue(obj, kvp.Key, kvp.Value);
            }
        }

        public static IEnumerable<Attribute> GetAttributes<T>(T obj) {
            return (IEnumerable<Attribute>)typeof(T).GetCustomAttributes(true);
        }

        /**
         * Return a FieldInfo collection where each entry is checked against IsSpecialName and HasIgnoredAttribute.
         */
        public static IEnumerable<FieldInfo> GetSerializableFields(Type type, BindingFlags flags) {
            return type.GetFields(flags).Where(x =>
                ((flags & BindingFlags.Public) == 0 || !x.IsPrivate) &&
                !HasIgnoredAttribute(x));
        }

        /**
         * Return a PropertyInfo collection where each entry is checked against IsSpecialName and HasIgnoredAttribute.
         */
        public static IEnumerable<PropertyInfo> GetSerializableProperties(Type type, BindingFlags flags) {
            // check that:
            // 	- setter exists
            // 	- flags don't care about public, or they do but setter is public anyways
            // 	- it's not special
            //	- it's not tagged as obsolete or ignore 
            return type.GetProperties(flags).Where(
                x => x.CanWrite &&
                     ((flags & BindingFlags.Public) == 0 || x.GetSetMethod() != null) &&
                     !x.IsSpecialName &&
                     !HasIgnoredAttribute(x));
        }

        /**
         * Get the value for a property or field from an object with it's name.
         */
        public static T GetValue<T>(object obj, string name) {
            return GetValue<T>(obj, name, BindingFlags.Instance | BindingFlags.Public);
        }

        /**
         * Get the value for a property or field from an object with it's name.
         */
        public static T GetValue<T>(object obj, string name, BindingFlags flags) {
            if (obj == null) {
                return default(T);
            }

            var prop = obj.GetType().GetProperty(name, flags);

            if (prop != null) {
                return (T)prop.GetValue(obj, null);
            }

            var field = obj.GetType().GetField(name, flags);

            if (field != null) {
                return (T)field.GetValue(obj);
            }

            return default(T);
        }

        /**
         * Returns true if reflected info finds any deprecated or obsolete attributes.
         */
        public static bool HasIgnoredAttribute(Type type) {
            return type.GetCustomAttributes(true).Any(x => IgnoreAttributes.Contains(x.GetType()));
        }

        public static bool HasIgnoredAttribute<T>(T info) where T : MemberInfo {
            return info.GetCustomAttributes(true).Any(x => IgnoreAttributes.Contains(x.GetType()));
        }

        /**
         * Iterate all public instance fields on an object and store them in a dictionary
         * with property/field name as the key, and value as value.
         */
        public static Dictionary<string, object> ReflectProperties<T>(T obj) {
            return ReflectProperties(obj, BindingFlags.Instance | BindingFlags.Public, null);
        }

        public static T CopyFields<T>(T source) {
            return JsonUtility.FromJson<T>(JsonUtility.ToJson(source));
        }

        public static void CopyFields<T>(T source, T dest) where T : UnityEngine.Object {
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(source), dest);
        }

        public static Dictionary<string, object> ReflectProperties<T>(T obj, HashSet<string> ignoreFields) {
            return ReflectProperties(obj, BindingFlags.Instance | BindingFlags.Public, ignoreFields);
        }

        public static Dictionary<string, object> ReflectProperties<T>(T obj, BindingFlags flags, HashSet<string> ignoreFields) {
            System.Text.StringBuilder sb = null;
            if (Debugging) {
                sb = new System.Text.StringBuilder();
                sb.AppendLine("Component type(" + obj.GetType().ToString() + ")");
            }

            var properties = new Dictionary<string, object>();

            var type = obj.GetType();

            foreach (var prop in GetSerializableProperties(type, flags)) {
                try {
                    var willWrite = prop.CanWrite &&
                                    !prop.IsSpecialName &&
                                    !HasIgnoredAttribute(prop) &&
                                    !(ignoreFields != null && ignoreFields.Contains(prop.Name));

                    if (Debugging) {
                        sb.AppendLine((willWrite ? "p - " : "px- ") + prop.Name + " : " + prop.GetValue(obj, null));
                    }

                    if (willWrite) {
                        var parms = prop.GetIndexParameters();

                        if (parms != null && parms.Length > 0) {
                            // Debug.LogWarning("ParameterInfo[] > 0 : " + prop.Name);
                        }
                        else {
                            properties.Add(prop.Name, prop.GetValue(obj, parms));
                        }
                    }
                }
                catch { }
            }

            foreach (var field in type.GetFields(flags)) {
                try {
                    if (Debugging) {
                        sb.AppendLine((HasIgnoredAttribute(field) ? "fx - " : "f  - ") + field.Name + " : " + field.GetValue(obj));
                    }

                    if (HasIgnoredAttribute(field) ||
                        ignoreFields != null && ignoreFields.Contains(field.Name)) {
                        continue;
                    }

                    properties.Add(field.Name, field.GetValue(obj));
                }
                catch {
                    Debug.LogError("Failed extracting property: " + field.Name);
                }
            }

            if (Debugging) {
                Debug.Log(sb.ToString());
            }

            return properties;
        }

        /**
         * Attempt to set a value with Field and target object.  Handles converting from object to actual type, and covers
         * some serialization "gotchas" when coming from JSON.
         */
        public static bool SetFieldValue(object target, FieldInfo field, object value) {
            if (target == null || field == null) {
                return false;
            }

            try {
                field.SetValue(target, ConvertValue(value, field.FieldType));
                return true;
            }
            catch (Exception e) {
                Debug.LogErrorFormat("{0} field {1} value {2} valueType {3}: {4}", target.ToString(), field.Name, value.ToString(), value.GetType(), e.ToString());
                return false;
            }
        }

        /**
         * Attempt to set a value with PropertyInfo and target object.  Handles converting from object to actual type, and covers
         * some serialization "gotchas" when coming from JSON.
         * SetValue and it's overrides end up calling this or SetFieldValue.
         */
        public static bool SetPropertyValue(object target, PropertyInfo propertyInfo, object value) {
            if (propertyInfo == null || target == null) {
                return false;
            }

            try {
                propertyInfo.SetValue(target, ConvertValue(value, propertyInfo.PropertyType), null);
                return true;
            }

			catch(System.Exception e) {
                //Debug.LogWarning(e.ToString());
			    if (Debugging) {
			        Debug.LogErrorFormat("{0} property {1} value {2}: {3}", target.ToString(), propertyInfo.Name, value.ToString(), e.ToString());
			    }
			    return false;
			}
        }

        private static BindingFlags _flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static System.Object ConvertValue(System.Object value, Type type) {
            var val = value is JToken ? ((JToken)value).ToObject(type, Serializer.JsonCustom) : value;
            if (type.IsEnum) {
                return (int)Convert.ChangeType(val, typeof(int));
            }
            if (val is ISerializableObjectWrapper) {
                var actual = ((ISerializableObjectWrapper)val).GetValue();
                if (actual == null) {
                    return null;
                }
                var component = actual as UnityEngine.Component;
                if (component != null) {
                    return component.gameObject.GetComponent(type);
                }
                return Convert.ChangeType(actual, type);
            }
            if (type == typeof(bool)) {
                bool final;
                if (!Boolean.TryParse(val as string, out final)) {
                    int intVal;
                    if (int.TryParse(val as string, out intVal)) {
                        final = intVal != 0;
                    }
                }
                return final;
            }
            return Convert.ChangeType(val, type);
        }

        public static bool SetValue(object obj, string name, object value) {
            if (obj == null) {
                return false;
            }
            var type = obj.GetType();
            var prop = type.GetProperty(name, _flags);
            if (prop != null) {
                return SetPropertyValue(obj, prop, value);
            }
            var field = GetField(type, name);
            if (field != null) {
                return SetFieldValue(obj, field, value);
            }
            return false;
        }

        public static bool ResizeArray(object obj, string name, int size) {
            if (obj == null) {
                return false;
            }
            var array = GetArray(obj, name);
            if (array == null) {
                return false;
            }
            Type elementType = array.GetType().GetElementType();
            Array newArray = Array.CreateInstance(elementType, size);
            Array.Copy(array, newArray, MathEx.Min(array.Length, newArray.Length));
            SetArray(obj, name, newArray);
            return false;
        }

        public static bool SetArrayObj(object obj, string name, int index, object val) {
            if (obj == null) {
                return false;
            }
            var array = GetArray(obj, name);
            if (array == null || index >= array.Length) {
                return false;
            }
            try {
                var target = ConvertValue(val, array.GetType().GetElementType());
                array.SetValue(target, index);
                SetArray(obj, name, array);
                return true;
            }

            catch (System.Exception e) {
                if (Debugging) {
                    //Debug.LogWarning(e.ToString());
                    Debug.LogErrorFormat("{0} property {1} value {2} index {3}: {4}", obj.ToString(), array.ToString(), val.ToString(), index, e.ToString());
                }
                return false;
            }
        }

        private static Array GetArray(object obj, string name) {
            var type = obj.GetType();
            var prop = type.GetProperty(name, _flags);
            Array array = null;
            if (prop != null) {
                array = prop.GetValue(obj, null) as Array;
            }
            if (array != null) {
                return array;
            }
            FieldInfo field = GetField(type, name);
            if (field != null) {
                return field.GetValue(obj) as Array;
                
            }
            return null;
        }

        private static bool SetArray(object obj, string name, Array newArray) {
            var type = obj.GetType();
            var prop = type.GetProperty(name, _flags);
            Array array = null;
            if (prop != null) {
                array = prop.GetValue(obj, null) as Array;
            }
            if (array != null) {
                prop.SetValue(obj, newArray, null);
                return true;
            }
            FieldInfo field = GetField(type, name);
            if (field != null) {
                field.SetValue(obj, newArray);
                return true;

            }
            return false;
        }

        private static FieldInfo GetField(System.Type type, string name) {
            FieldInfo field = type.GetField(name, _flags);
            if (field != null) {
                return field;
            }
            int limit = 0;
            while (type.BaseType != null) {
                field = type.BaseType.GetField(name, _flags);
                if (field != null) {
                    return field;
                }
                type = type.BaseType;
                limit++;
                if (limit > 500) {
                    break;
                }
            }
            return null;
        }

        public delegate T ObjectActivator<T>(params object[] args);

        public static ObjectActivator<T> GetConstructor<T>
            (ConstructorInfo ctor) {
            Type type = ctor.DeclaringType;
            ParameterInfo[] paramsInfo = ctor.GetParameters();

            //create a single param of type object[]
            ParameterExpression param =
                Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp =
                new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (int i = 0; i < paramsInfo.Length; i++) {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp =
                    Expression.ArrayIndex(param, index);

                Expression paramCastExp =
                    Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            NewExpression newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            LambdaExpression lambda =
                Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

            //compile it
            ObjectActivator<T> compiled = (ObjectActivator<T>) lambda.Compile();
            return compiled;
        }

    }
}
