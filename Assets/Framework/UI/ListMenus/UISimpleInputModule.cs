using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace PixelComrades {
    public class UISimpleInputModule : StandaloneInputModule {

        public GameObject GameObjectUnderPointer(int pointerId) {
            var lastPointer = GetLastPointerEventData(pointerId);
            if (lastPointer != null)
                return lastPointer.pointerCurrentRaycast.gameObject;
            return null;
        }

        public GameObject GameObjectUnderPointer() {
            return GameObjectUnderPointer(PointerInputModule.kMouseLeftId);
        }
    }
}
