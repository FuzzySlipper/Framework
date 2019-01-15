using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {
    //:ISerializable 
    /// <summary>
    /// Important: If components inherit from each other they will not be found by their base component
    /// </summary>
    public interface IComponent  {
        int Owner { get; set; }
        //TODO: mandatory Dispose and Serialize

    }

}