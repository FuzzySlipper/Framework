using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    
    public struct ComponentReference : IComparable<ComponentReference>, IEquatable<ComponentReference> {
        public int Index { get; }
        public ManagedArray Array { get; }

        public ComponentReference(int index, ManagedArray array) {
            Index = index;
            Array = array;
        }

        public System.Object Get() {
            return Array.Get(Index);
        }

        public ref T Get<T>() {
            return ref ((ManagedArray<T>) Array)[Index];
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            return obj is ComponentReference other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (Index * 397) ^ (Array != null ? Array.GetHashCode() : 0);
            }
        }

        public int CompareTo(ComponentReference other) {
            return other.Index.CompareTo(Index);
        }

        public bool Equals(ComponentReference other) {
            return other.Index == Index && other.Array == Array;
        }
    }

    public static class ComponentReferenceExtensions {

        public static Entity GetEntity<T>(this T component) where T : IComponent {
            return EntityController.GetComponentArray<T>().GetEntity(component);
        }
        
        //public static bool ContainsType(this IList<ComponentReference> compList, System.Type type) {
        //    for (int i = 0; i < compList.Count; i++) {
        //        if (compList[i].Type == type) {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //public static bool ContainsDerivedType(this IList<ComponentReference> compList, System.Type type) {
        //    for (int i = 0; i < compList.Count; i++) {
        //        if (compList[i].Type.IsAssignableFrom(type)) {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //public static bool ContainsDerivedType(this Dictionary<Type, ComponentReference> compList, System.Type type) {
        //    foreach (var cref in compList) {
        //        if (type.IsAssignableFrom(cref.Key)) {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }
}
