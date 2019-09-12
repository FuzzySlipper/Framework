using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class RigidbodyComponent : IComponent, IDisposable {

        private CachedUnityComponent<Rigidbody> _component;
        public Rigidbody Rb { get { return _component.Component; } }

        public RigidbodySettings RigidbodySetup;

        public RigidbodyComponent(Rigidbody rb) {
            RigidbodySetup = new RigidbodySettings();
            _component = new CachedUnityComponent<Rigidbody>(rb);
            if (Rb != null) {
                RigidbodySetup.Setup(Rb);
            }
        }

        public RigidbodyComponent(SerializationInfo info, StreamingContext context) {
            _component = info.GetValue(nameof(_component), _component);
            RigidbodySetup = new RigidbodySettings();
            if (Rb != null) {
                RigidbodySetup.Setup(Rb);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_component), _component);
        }
        
        public void SetRb(Rigidbody rb) {
            _component = new CachedUnityComponent<Rigidbody>(rb);
            if (Rb != null) {
                RigidbodySetup.Setup(Rb);
            }
        }

        public Vector3 position {
            get { return Rb.position; }
            set { Rb.position = value; }
        }

        public Vector3 velocity {
            get { return Rb.velocity; }
            set { Rb.velocity = value; }
        }

        public void AddForce(Vector3 force) {
            Rb.AddForce(force);
        }

        public void AddTorque(Vector3 force) {
            Rb.AddTorque(force);
        }

        public void Dispose() {
            RigidbodySetup = null;
            _component = null;
        }
    }
}
