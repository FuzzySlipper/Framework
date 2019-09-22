using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class TransformComponent : IComponent {

        private CachedTransform _cachedTransform;
        
        private Transform Value { get => _cachedTransform.Tr; }
        public bool IsValid { get => _cachedTransform.Tr != null; }
        public GameObject gameObject { get => Value.gameObject; }
        public Vector3 position { get => Value.position; }
        public Vector3 localPosition { get => Value.localPosition; }
        public Vector3 forward { get => Value.forward; }
        public Vector3 up { get => Value.up; }
        public Vector3 right { get => Value.right; }
        public Vector3 scale { get => Value.localScale; }
        public Quaternion rotation { get => Value.rotation; }
        public Quaternion localRotation { get => Value.localRotation; }

        public TransformComponent(Transform tr) {
            _cachedTransform = new CachedTransform(tr);
        }

        public void Set(Transform tr) {
            _cachedTransform.Set(tr);
        }

        public Vector3 InverseTransformPoint(Vector3 pos) {
            return Value.InverseTransformPoint(pos);
        }

        public Vector3 InverseTransformVector(Vector3 dir) {
            return Value.InverseTransformVector(dir);
        }

        public Vector3 InverseTransformDirection(Vector3 dir) {
            return Value.InverseTransformDirection(dir);
        }
        
        public Vector3 TransformPoint(Vector3 pos) {
            return Value.TransformPoint(pos);
        }

        public Directions ForwardDirection2D() {
            return Value.ForwardDirection2D();
        }

        public void SetParent(Transform other) {
            Value.SetParent(other);
        }

        public void SetChild(Transform other) {
            other.SetParent(Value);
        }
        
        /// <summary>
        /// Set transform position, use only in TransformSystem
        /// </summary>
        /// <param name="pos"></param>
        public void SetPosition(Vector3 pos) {
            Value.position = pos;
        }

        /// <summary>
        /// Set transform rotation, use only in TransformSystem
        /// </summary>
        /// <param name="rot"></param>
        public void SetRotation(Quaternion rot) {
            Value.rotation = rot;
        }

        /// <summary>
        /// Set transform position, use only in TransformSystem
        /// </summary>
        /// <param name="pos"></param>
        public void SetLocalPosition(Vector3 pos) {
            Value.localPosition = pos;
        }

        /// <summary>
        /// Set transform rotation, use only in TransformSystem
        /// </summary>
        /// <param name="rot"></param>
        public void SetLocalRotation(Quaternion rot) {
            Value.localRotation = rot;
        }
        
        public TransformComponent(SerializationInfo info, StreamingContext context) {
            _cachedTransform = info.GetValue(nameof(_cachedTransform), _cachedTransform);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_cachedTransform), _cachedTransform);
        }

//        public static implicit operator Transform(TransformComponent component) {
//            if (component == null) {
//                return null;
//            }
//            return component.Value;
//        }
    }
}
