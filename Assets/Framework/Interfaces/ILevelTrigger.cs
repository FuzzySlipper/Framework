using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface ILevelTrigger {
        string Id { get; }
        bool ActiveStatus { get; set; }
        Point3 GridPos { get; }
    }
}
