using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct DamageEvent : IEntityMessage {
        public float Amount;
        public Entity Origin { get; }
        public Entity Target { get; }
        public int DamageType { get; }
        public int TargetVital { get; }

        public DamageEvent(float amount, Entity origin, Entity target, int damageType, int targetVital) {
            Amount = amount;
            Origin = origin;
            Target = target;
            DamageType = damageType;
            TargetVital = targetVital;
        }

        public string ToDescription() {
            return string.Format("{0:F0} {1}", Amount, DamageTypes.GetDescriptionAt(DamageType));
        }
    }

    public struct HealEvent : IEntityMessage {
        public float Amount;
        public Entity Origin { get; }
        public Entity Target { get; }
        public int TargetVital { get; }

        public HealEvent(float amount, Entity origin, Entity target, int targetVital) {
            Amount = amount;
            Origin = origin;
            Target = target;
            TargetVital = targetVital;
        }
    }

    public struct DeathEvent : IEntityMessage {
        public Entity Caused { get; }
        public Entity Target { get; }
        
        public DeathEvent(Entity caused, Entity target) {
            Caused = caused;
            Target = target;
        }
    }

    public struct RaiseDead : IEntityMessage {
        public Entity Target { get; }

        public RaiseDead(Entity target) {
            Target = target;
        }
    }

    public struct FloatingTextMessage : IEntityMessage {
        public string Message;
        public Color Color;
        public Entity Target;

        public FloatingTextMessage(string message, Color color, Entity target) {
            Message = message;
            Color = color;
            Target = target;
        }
    }
}
