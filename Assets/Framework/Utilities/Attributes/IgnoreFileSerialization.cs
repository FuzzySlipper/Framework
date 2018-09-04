using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class IgnoreFileSerialization : Attribute { }
}
