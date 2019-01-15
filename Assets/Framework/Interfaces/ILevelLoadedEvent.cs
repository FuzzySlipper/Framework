using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface ILevelLoadEvents {
        void LevelLoaded(List<SerializedGenericData> mapData);
        void LevelSaved(ref List<SerializedGenericData> mapData);
    }
}
