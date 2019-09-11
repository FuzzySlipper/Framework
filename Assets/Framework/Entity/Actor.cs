using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class Actor : Entity, IEquatable<Actor> {
        
        protected Actor() : base() {}
        
        public bool Equals(Actor other) {
            return other != null && other.Id == Id;
        }
    }
}
