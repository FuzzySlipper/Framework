using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class BlockDamageAction : IComponent {

        public GameObject ModelData;
        public string TargetVital;
        public float Cost;
        public string Skill;
        public string ChargeInput;

        public BlockDamageAction(GameObject modelData, string targetVital, float cost, string skill, string chargeInput) {
            ModelData = modelData;
            TargetVital = targetVital;
            Cost = cost;
            Skill = skill;
            ChargeInput = chargeInput;
        }

        public BlockDamageAction(SerializationInfo info, StreamingContext context) {
            //CurrentCharge = info.GetValue(nameof(CurrentCharge), CurrentCharge);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(CurrentCharge), CurrentCharge);
        }
    }
}