using UnityEngine;

namespace PixelComrades {

    public enum ActionStateEvents {
        Start = 0, Activate = 1, Miss = 2, Impact = 3, None
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