using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface INode {
        void Register(Entity entity, Dictionary<System.Type, ComponentReference> list);
        void Dispose();
    }
}
