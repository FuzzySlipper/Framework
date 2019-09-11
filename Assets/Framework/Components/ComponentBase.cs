using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace PixelComrades {
    public abstract class ComponentBase : IComponent {
        public Entity Entity { get{ return this.GetEntity();} }
        public int Owner { get { return Entity.Id;} }
        public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
    }
}
