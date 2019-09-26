using UnityEngine;

namespace PixelComrades {

    public enum ActionStateEvents {
        Start = 0, Activate = 1, Miss = 2, Impact = 3, Collision = 4, CollisionOrImpact = 5, FxOn = 6, FxOff = 7, None

        //Start, Activate, Miss, Impact, Collision, CollisionOrImpact, FxOn, FxOff, None
    }

    public struct SimpleStat {
        public string ID;
        public float Amount;
    }

    public struct ActionStateEvent: IEntityMessage {
        public CharacterNode Origin { get; }
        public CharacterNode Target { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public ActionStateEvents State { get; }

        public ActionStateEvent(CharacterNode origin, CharacterNode target, Vector3 position, Quaternion rotation, ActionStateEvents state) {
            Origin = origin;
            Target = target;
            Position = position;
            Rotation = rotation;
            State = state;
        }

        public ActionStateEvent(BaseNode origin, BaseNode focus, Vector3 position, Quaternion rotation, ActionStateEvents state) {
            Origin = origin.Entity.FindNode<CharacterNode>();
            Target = focus.Entity.FindNode<CharacterNode>();
            Position = position;
            Rotation = rotation;
            State = state;
        }

        public ActionStateEvent(Entity origin, Entity focus, Vector3 position, Quaternion rotation, ActionStateEvents state) {
            Origin = origin.FindNode<CharacterNode>();
            Target = focus.FindNode<CharacterNode>();
            Position = position;
            Rotation = rotation;
            State = state;
        }
    }
}