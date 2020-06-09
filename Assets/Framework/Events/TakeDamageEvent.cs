using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public struct DamageEntry {
        public float Amount;
        public string DamageType;
        public string TargetVital;
        public string Description;

        public DamageEntry(float amount, string damageType, string targetVital, string description) {
            Amount = amount;
            DamageType = damageType;
            TargetVital = targetVital;
            Description = description;
        }

        public string ToDescription() {
            return string.Format("{0:F0} {1}", Amount, DamageType);
        }
    }
    
    public struct TakeDamageEvent : IRuleEvent {
        public HitData Hit { get; }
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public float Amount { get; }
        public ActionTemplate Action { get; }
        public List<DamageEntry> Entries;

        public TakeDamageEvent(ref PrepareDamageEvent prepareEvent) {
            Action = prepareEvent.Action;
            Origin = prepareEvent.Origin;
            Target = prepareEvent.Target;
            Hit = prepareEvent.Hit;
            Entries = prepareEvent.Entries;
            Amount = prepareEvent.CurrentTotal();
            prepareEvent.Entries = null;
        }

        public void Clear() {
            GenericPools.Store(Entries);
            Entries = null;
        }
    }

    public struct PrepareDamageEvent : IRuleEvent {
        public HitData Hit { get; }
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public ActionTemplate Action { get; }
        public List<DamageEntry> Entries;

        public float CurrentTotal() {
            float amt = 0;
            for (int i = 0; i < Entries.Count; i++) {
                amt += Entries[i].Amount;
            }
            return amt;
        }

        public PrepareDamageEvent(ImpactEvent impact) {
            Entries = GenericPools.New<List<DamageEntry>>();
            Origin = impact.Origin;
            Target = impact.Target;
            Action = impact.Action;
            Hit = impact.Hit;
        }

        public PrepareDamageEvent(CharacterTemplate origin, CharacterTemplate target, ActionTemplate action, HitData hit) {
            Entries = GenericPools.New<List<DamageEntry>>(); 
            Origin = origin;
            Target = target;
            Action = action;
            Hit = hit;
        }
    }

    public struct CausedDamageEvent : IEntityMessage {
        public float Amount { get; }
        public CharacterTemplate Origin { get { return TakeDamage.Origin; } }
        public CharacterTemplate Target { get { return TakeDamage.Target; } }
        public ActionTemplate Action { get { return TakeDamage.Action; } }
        public TakeDamageEvent TakeDamage { get; }

        public CausedDamageEvent(float amount, TakeDamageEvent damageEvent) {
            TakeDamage = damageEvent;
            Amount = amount;
        }
    }

    public struct ReceivedDamageEvent : IEntityMessage {
        public float Amount { get; }
        public CharacterTemplate Origin { get { return TakeDamage.Origin; } }
        public CharacterTemplate Target { get { return TakeDamage.Target; } }
        public ActionTemplate Action { get { return TakeDamage.Action; } }
        public TakeDamageEvent TakeDamage { get; }

        public ReceivedDamageEvent(float amount, TakeDamageEvent damageEvent) {
            TakeDamage = damageEvent;
            Amount = amount;
        }
    }

    public struct HealingEvent : IRuleEvent {
        public float Amount { get; }
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public ActionTemplate Action { get; }
        public string TargetVital { get; }

        public HealingEvent(ActionTemplate action, float amount, CharacterTemplate origin, CharacterTemplate target, string targetVital) {
            Action = action;
            Amount = amount;
            Origin = origin;
            Target = target;
            TargetVital = targetVital;
        }
    }

    public struct DeathEvent : IEntityMessage {
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public Vector3 HitPoint { get; }
        public float OverKill { get; }

        public DeathEvent(CharacterTemplate caused, CharacterTemplate target, Vector3 hitPoint, float overKill) {
            Origin = caused;
            Target = target;
            OverKill = overKill;
            HitPoint = hitPoint;
        }
    }

    public struct RaiseDeadEvent : IEntityMessage {
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public ActionTemplate Action { get; }

        public RaiseDeadEvent(ActionTemplate action, CharacterTemplate source, CharacterTemplate target) {
            Action = action;
            Origin = source;
            Target = target;
        }
    }
}
