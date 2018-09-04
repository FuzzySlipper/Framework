using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PixelComrades {
    public abstract class MapSystem : SystemBase {

        public abstract bool LevelPositionIsFree(Point3 pos);
        public abstract BaseCell GetCell(Point3 pos);
        public abstract int Level { get; }
    }
}