using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {
    //:ISerializable 
    public interface IComponent  {
        int Owner { get; set; }
        //TODO: mandatory Dispose and Serialize
    }

}