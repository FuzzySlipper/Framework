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
            return string.Format("{0:F0} {1}", Defenses.GetNameAt(DamageType));
        }
    }
    
    public struct TakeDamageEvent : IRuleEvent {
        public ImpactEvent Impact { get; }
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public float Amount { get; }
        public ActionTemplate Action { get; }
        public List<DamageEntry> Entries;

        public TakeDamageEvent(ref PrepareDamageEvent prepareEvent) {
            Action = prepareEvent.Action;
            Origin = prepareEvent.Origin;
            Target = prepareEvent.Target;
            Impact = prepareEvent.Impact;
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
        public ImpactEvent Impact { get; }
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

        public PrepareDamageEvent(ImpactEvent impact, CharacterTemplate origin, CharacterTemplate target) {
            Entries = GenericPools.New<List<DamageEntry>>(); 
            Impact = impact;
            Origin = origin;
            Target = target;
            Action = impact.Action;
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

        public HealingEvent(float amount, CharacterTemplate origin, CharacterTemplate target, string targetVital) {
            Action = origin.CurrentAction;
            Amount = amount;
            Origin = origin;
            Target = target;
            TargetVital = targetVital;
        }
    }

    public struct DeathEvent : IEntityMessage {
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public ActionTemplate Action { get; }
        public ImpactEvent Impact { get; }
        public float OverKill { get; }

        public DeathEvent(CharacterTemplate caused, CharacterTemplate target, ImpactEvent impact, float overKill) {
            Action = impact.Action;
            Origin = caused;
            Target = target;
            OverKill = overKill;
            Impact = impact;
        }
    }

    public struct RaiseDeadEvent : IEntityMessage {
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public ActionTemplate Action { get; }

        public RaiseDeadEvent(CharacterTemplate source, CharacterTemplate target) {
            Action = source.CurrentAction;
            Origin = source;
            Target = target;
        }
    }
}
