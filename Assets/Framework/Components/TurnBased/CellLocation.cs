using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class CellLocation : IComponent, IDisposable {

        public LevelCell Cell;

        public CellLocation() { }

        public CellLocation(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }

        public void Dispose() {
            if (Cell != null && Cell.Unit == this.GetEntity()) {
                Cell.Unit = null;
            }
        }

        public static implicit operator LevelCell(CellLocation reference) {
            return reference?.Cell;
        }
    }
}