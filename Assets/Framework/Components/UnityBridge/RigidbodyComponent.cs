using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct RigidbodyComponent : IComponent, IDisposable {

        public int Owner { get; set; }
        public Rigidbody Rb;

        public RigidbodySettings RigidbodySetup;

        public RigidbodyComponent(Rigidbody rb) : this() {
            Rb = rb;
            RigidbodySetup = new RigidbodySettings();
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
            Rb = null;
        }
    }
}
