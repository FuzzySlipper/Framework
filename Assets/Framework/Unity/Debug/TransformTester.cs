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

        public Transform NeighborTester = null;

        #if UNITY_EDITOR

        private void OnDrawGizmosSelected() {
            if (NeighborTester == null) {
                return;
            }
            Gizmos.DrawWireCube(NeighborTester.position, Vector3.one);
            var pos = transform.position.WorldToGenericGrid(5);
            var neighbor = NeighborTester.transform.position.WorldToGenericGrid(5);
            UnityEditor.Handles.Label(transform.position, string.Format("{0}-{1} Neighbor pos {2} neighbor {3}", pos, neighbor, pos.IsNeighbor(neighbor), neighbor.IsNeighbor(pos)));
        }

#endif
    }
}
