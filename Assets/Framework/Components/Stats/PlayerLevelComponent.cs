using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class PlayerLevelComponent : IComponent {

        public int SkillPoints;
<<<<<<< HEAD
        public int AttributePoints;
=======
>>>>>>> FirstPersonAction
        public ExperienceStat Xp = new ExperienceStat();
        public PlayerLevelComponent(){}
        
        public PlayerLevelComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
