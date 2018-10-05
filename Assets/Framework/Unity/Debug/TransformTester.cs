using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class TransformTester : MonoBehaviour {

        [Button("Reset Position")]
        public void ResetPosition() {
            transform.localPosition = Vector3.zero;
        }

        [Button("Reset Anchored Position")]
        public void ResetAnchored() {
            ((RectTransform) (transform)).anchoredPosition = Vector2.zero;
        }

        [Button("Set Anchor LL")]
        public void SetAnchor0() {
            ((RectTransform)(transform)).SetAnchorsAndPivots(Vector2.zero);
        }

        [Button("Set Anchor UL")]
        public void SetAnchor1() {
            ((RectTransform) (transform)).SetAnchorsAndPivots(new Vector2(0,1));
        }

        [Button("Set Anchor UR")]
        public void SetAnchor2() {
            ((RectTransform) (transform)).SetAnchorsAndPivots(Vector2.one);
        }

        [Button("Set Anchor LR")]
        public void SetAnchor3() {
            ((RectTransform) (transform)).SetAnchorsAndPivots(new Vector2(1, 0));
        }
    }
}
