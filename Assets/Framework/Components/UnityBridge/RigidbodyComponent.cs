using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class RigidbodyComponent : IComponent, IDisposable {

        public int Owner { get; set; }
        public Rigidbody Rb { get; private set; }

        public RigidbodySettings RigidbodySetup;

        public RigidbodyComponent(Rigidbody rb) {
            RigidbodySetup = new RigidbodySettings();
            Rb = rb;
            if (Rb != null) {
                RigidbodySetup.Setup(Rb);
            }
        }

        public void SetRb(Rigidbody rb) {
            Rb = rb;
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
