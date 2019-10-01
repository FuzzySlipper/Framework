using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class ChargeComponent : IComponent {
        public float CurrentCharge;

        public ChargeComponent(){}
        
        public ChargeComponent(SerializationInfo info, StreamingContext context) {
            CurrentCharge = info.GetValue(nameof(CurrentCharge), CurrentCharge);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(CurrentCharge), CurrentCharge);
        }
    }

    [Serializable]
    public class WaitForCharge : ActionLayer, ISerializable  {
        public FloatRange ForceRange { get; }
        public float MaxChargeTime { get; }
        public string ChargeInput { get; }

        private PlayerInputComponent _input;
        private float _start;

        public WaitForCharge(Action action, string input, float maxChargeTime = 2, float min = 100, float max = 4000) : base(action) {
            ForceRange = new FloatRange(min, max);
            MaxChargeTime = maxChargeTime;
            ChargeInput = input;
        }

        public WaitForCharge(SerializationInfo info, StreamingContext context) : base(info, context) {
            ForceRange = info.GetValue(nameof(ForceRange), ForceRange);
            MaxChargeTime = info.GetValue(nameof(MaxChargeTime), MaxChargeTime);
            ChargeInput = info.GetValue(nameof(ChargeInput), ChargeInput);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ForceRange), ForceRange);
            info.AddValue(nameof(MaxChargeTime), MaxChargeTime);
            info.AddValue(nameof(ChargeInput), ChargeInput);
        }
        
        public override void Start(ActionUsingNode node) {
            base.Start(node);
            _start = TimeManager.Time;
            _input = node.Entity.Get<PlayerInputComponent>();
        }

        public override void Evaluate(ActionUsingNode node) {
            var elapsed = (TimeManager.Time - _start);
            if (elapsed >= MaxChargeTime || ( _input != null && !_input.Handler.GetButton(ChargeInput))) {
                var chargeComponent = node.ActionEvent.Action.Entity.GetOrAdd<ChargeComponent>();
                chargeComponent.CurrentCharge = ForceRange.Lerp(Mathf.Clamp01(elapsed / MaxChargeTime));
                node.AdvanceEvent();
            }
        }
    }
}
