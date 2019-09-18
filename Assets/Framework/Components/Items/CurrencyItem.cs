using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class CurrencyItem : IComponent {

        public CurrencyItem(SerializationInfo info, StreamingContext context) {
            _count = info.GetValue(nameof(_count), _count);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_count), _count);
        }
        
        private int _count;
        
        public int Count => _count;

        public CurrencyItem(int count) {
            _count = count;
        }
    }
}
