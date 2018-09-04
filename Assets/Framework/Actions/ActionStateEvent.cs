using UnityEngine;

namespace PixelComrades {

    public enum ActionStateEvents {
        Start, Activate, Collision, AppliedImpact, Miss, None
    }

    public struct ActionStateEvent: IEntityMessage {
        public int Origin;
        public int Focus;
        public Vector3 Position;
        public Quaternion Rotation;
        public ActionStateEvents State;

        public ActionStateEvent(int origin, int focus, Vector3 position, Quaternion rotation, ActionStateEvents state) {
            Origin = origin;
            Focus = focus;
            Position = position;
            Rotation = rotation;
            State = state;
        }

        public Entity GetFocus() {
            return EntityController.GetEntity(Focus);
        }

        public Entity GetOrigin() {
            return EntityController.GetEntity(Origin);
        }
    }
}