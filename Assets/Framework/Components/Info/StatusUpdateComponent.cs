using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Highest)]
    public class StatusUpdateComponent : IComponent, IReceive<StatusUpdate> {

        public string Status;
        public int Owner { get; set; }

        public StatusUpdateComponent() {}

        public static implicit operator string(StatusUpdateComponent comp) {
            return comp?.Status;
        }

        public void Handle(StatusUpdate arg) {
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
