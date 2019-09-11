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
        public Entity Origin;
        public Entity Focus;
        public Vector3 Position;
        public Quaternion Rotation;
        public ActionStateEvents State;

        public ActionStateEvent(Entity origin, Entity focus, Vector3 position, Quaternion rotation, ActionStateEvents state) {
            Origin = origin;
            Focus = focus;
            Position = position;
            Rotation = rotation;
            State = state;
        }
    }
}