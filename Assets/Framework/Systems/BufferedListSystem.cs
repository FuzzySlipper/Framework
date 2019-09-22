using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister, Priority(Priority.Highest)]
    public sealed class BufferedListSystem : SystemBase, IMainSystemUpdate {
        
        public BufferedListSystem(){}
        
        public void OnSystemUpdate(float dt, float unscaledDt) {
            BufferedList.UpdateAllLists();
        }
    }
}
