using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FlightEngineConfig : ScriptableObject {
        [Header("Forces")] 
        public Vector3 DefaultRotationForces = new Vector3(8f, 8f, 18f);
        public Vector3 DefaultTranslationForces = new Vector3(1200, 1200, 2400);
        public Vector3 DefaultBoostForces = new Vector3(1600, 1600, 4800);
        public float MaxMulti = 4f;
    }
}
