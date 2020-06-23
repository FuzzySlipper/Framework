<<<<<<< HEAD
﻿using UnityEngine;
=======
﻿﻿using UnityEngine;
>>>>>>> FirstPersonAction
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class InitiativeBonus : IComponent {

        public int Bonus;
        
        public InitiativeBonus(){}
        
        public InitiativeBonus(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
