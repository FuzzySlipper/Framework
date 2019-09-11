using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct DamageEvent : IEntityMessage {
        public float Amount;
        public Entity Origin { get; }
        public Entity Target { get; }
        public string DamageType { get; }
        public string TargetVital { get; }

        public DamageEvent(float amount, Entity origin, Entity target, string damageType, string targetVital) {
            Amount = amount;
            Origin = origin;
            Target = target;
            DamageType = damageType;
            TargetVital = targetVital;
        }

        public string ToDescription() {
            return string.Format("{0:F0} {1}", Amount, GameData.DamageTypes.GetNameAt(DamageType));
        }
    }

    public struct HealEvent : IEntityMessage {
        public float Amount;
        public Entity Origin { get; }
        public Entity Target { get; }
        public string TargetVital { get; }

        public HealEvent(float amount, Entity origin, Entity target, string targetVital) {
            Amount = amount;
            Origin = origin;
            Target = target;
            TargetVital = targetVital;
        }
    }

    public struct DeathEvent : IEntityMessage {
        public Entity Caused { get; }
        public Entity Target { get; }
        public float OverKill { get; }

        public DeathEvent(Entity caused, Entity target, float overKill) {
            Caused = caused;
            Target = target;
            OverKill = overKill;
        }
    }

    public struct RaiseDead : IEntityMessage {
        public Entity Target { get; }

        public RaiseDead(Entity target) {
            Target = target;
        }
    }
}
