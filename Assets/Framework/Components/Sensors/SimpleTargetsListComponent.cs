using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    [System.Serializable]
    public sealed class SimpleTargetsListComponent : IComponent {
        
        public List<TurnBasedCharacterTemplate> TargetList = new List<TurnBasedCharacterTemplate>();
        
        public SimpleTargetsListComponent(){}
        
        public SimpleTargetsListComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
