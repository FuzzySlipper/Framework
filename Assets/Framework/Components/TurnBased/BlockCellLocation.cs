using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class BlockCellLocation : IComponent, IDisposable {

        public BlockCell Cell;

        public BlockCellLocation() { }

        public BlockCellLocation(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }

        public void Dispose() {
            if (Cell != null && Cell.Unit.Entity == this.GetEntity()) {
                Cell.Unit = null;
            }
        }

        public static implicit operator BlockCell(BlockCellLocation reference) {
            return reference?.Cell;
        }
    }
}