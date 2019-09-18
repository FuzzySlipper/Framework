using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Priority(Priority.Highest)]
    [System.Serializable]
	public sealed class StatusUpdateComponent : IComponent {

        public string Status;
        public StatusUpdateComponent() {}

        public StatusUpdateComponent(SerializationInfo info, StreamingContext context) {
            Status = info.GetValue(nameof(Status), Status);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Status), Status);
        }

        public static implicit operator string(StatusUpdateComponent comp) {
            return comp?.Status;
        }

    }

    public struct StatusUpdate : IEntityMessage {
        public string Update { get; }
        public Color Color { get; }
        public Entity Target { get; }

        public StatusUpdate(Entity target, string update, Color color) {
            Update = update;
            Color = color;
            Target = target;
        }

        public StatusUpdate(Entity target, string update) {
            Update = update;
            Color = Color.green;
            Target = target;
        }
    }

    public struct CombatStatusUpdate : IEntityMessage {
        public string Update { get; }
        public Color Color { get; }
        public Entity Target { get; }
        public CombatStatusUpdate(Entity target, string update, Color color) {
            Update = update;
            Color = color;
            Target = target;
        }

        public CombatStatusUpdate(Entity target, string update) {
            Update = update;
            Color = Color.green;
            Target = target;
        }
    }
}
