using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class StatusUpdateComponent : IComponent {

        public string Status;
        public int Owner { get; set; }

        public StatusUpdateComponent() {}

        public static implicit operator string(StatusUpdateComponent comp) {
            return comp?.Status;
        }
    }

    public struct StatusUpdate : IEntityMessage {
        public string Update;
        public Color Color;

        public StatusUpdate(string update, Color color) {
            Update = update;
            Color = color;
        }
    }
}
