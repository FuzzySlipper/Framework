using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct ActionEvent : IEntityMessage {
        public Entity Owner { get; }
        public Vector3 Target { get; }
        public Action Action { get; }
        public float TimeStart { get; }
        public int Index { get; }
        public Transform SpawnPivot { get; }

        public bool IsLastIndex { get { return Action.Sequence.Count - 1 == Index; } }
        public ActionLayer Current { get { return Index >= 0 && Index < Action.Sequence.Count ? Action.Sequence[Index] : null; } }

        public ActionEvent(Entity owner, Transform spawnPivot, Action action, Vector3 target) : this() {
            Owner = owner;
            Target = target;
            Action = action;
            SpawnPivot = spawnPivot;
            TimeStart = TimeManager.Time;
            Index = 0;
        }

        public ActionEvent(Entity owner, Action action, Vector3 target) : this() {
            Owner = owner;
            Target = target;
            Action = action;
            TimeStart = TimeManager.Time;
            Index = 0;
        }

        public ActionEvent(ActionEvent animEvent) : this() {
            Owner = animEvent.Owner;
            Target = animEvent.Target;
            Action = animEvent.Action;
            TimeStart = animEvent.TimeStart;
            SpawnPivot = animEvent.SpawnPivot;
            Index = animEvent.Index + 1;
        }
    }
}
