using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Priority(Priority.Highest)]
    public class StatusUpdateComponent : IComponent, IReceive<StatusUpdate> {

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

        public void Handle(StatusUpdate arg) {
#if DEBUG
            DebugLog.Add(this.GetEntity() + " received status " + arg.Update);
#endif
            Status = arg.Update;
        }
    }

    public struct StatusUpdate : IEntityMessage {
        public string Update;
        public Color Color;

        public StatusUpdate(string update, Color color) {
            Update = update;
            Color = color;
        }

        public StatusUpdate(string update) {
            Update = update;
            Color = Color.green;
        }
    }

    public struct CombatStatusUpdate : IEntityMessage {
        public string Update;
        public Color Color;

        public CombatStatusUpdate(string update, Color color) {
            Update = update;
            Color = color;
        }

        public CombatStatusUpdate(string update) {
            Update = update;
            Color = Color.green;
        }
    }
}
