using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {
    
    /// <summary>
    /// Important: If components inherit from each other they will not be found by their base component
    /// </summary>
    public interface IComponent : ISerializable {}
    
       //    public interface IComponentOnAttach {
       //        void OnAttach(Entity entity);
       //    }
       //
       //    public interface IComponentOnRemove {
       //        void OnRemove();
       //    }
}