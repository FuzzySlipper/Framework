using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface ISavedData {
        void SaveMetaData(SerializedMetaData data);
    }
}