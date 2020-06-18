using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class TurnBasedComponent : IComponent {

        public int TurnNumber = -1;
        public int InitiativeRoll = -1;
        public int InitiativeStatBonus;

        public int StandardActions;
        public int MinorActions;
        public int MoveActions;

        public int ActionPoints { get { return MinorActions + MoveActions + StandardActions; } }
        public float Speed { get { return InitiativeRoll + InitiativeStatBonus; } }

        public void Clear() {
            StandardActions = MinorActions = MoveActions = 0;
        }
        
        public TurnBasedComponent() {
        }

        public TurnBasedComponent(SerializationInfo info, StreamingContext context) {
            // _vital = info.GetValue(nameof(_vital), _vital);
            // _maxWeight = info.GetValue(nameof(_maxWeight), _maxWeight);
            // _speed = info.GetValue(nameof(_speed), _speed);
            // _inRecovery = info.GetValue(nameof(_inRecovery), _inRecovery);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            // info.AddValue(nameof(_vital), _vital);
            // info.AddValue(nameof(_maxWeight), _maxWeight);
            // info.AddValue(nameof(_speed), _speed);
            // info.AddValue(nameof(_inRecovery), _inRecovery);
        }

    }
}