using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class ActionPivotsComponent : IComponent {

        public Transform PrimaryPivot;
        public Transform SecondaryPivot;

        public ActionPivotsComponent(Transform primary, Transform secondary ) {
            PrimaryPivot = primary;
            SecondaryPivot = secondary;
        }

        public ActionPivotsComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }

<<<<<<< HEAD
    public partial class PivotTypes : StringEnum<PivotTypes> {
=======
    public class ActionPivotTypes : StringEnum<ActionPivotTypes> {
>>>>>>> FirstPersonAction
        public const string Primary = "Primary";
        public const string Secondary = "Secondary";
        public const string Hidden = "Hidden";
    }
}
