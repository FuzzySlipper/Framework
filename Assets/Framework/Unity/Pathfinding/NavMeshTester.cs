using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PixelComrades {
    public class NavMeshTester : MonoBehaviour {

        [SerializeField] private float _distance = 5;
        [SerializeField] private int _filterArea = -1;

#if UNITY_EDITOR
        void OnDrawGizmosSelected() {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, _distance, _filterArea)) {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, hit.position);
                Gizmos.DrawWireSphere(hit.position, 1.5f);
            }
            else {
                Handles.color = Color.red;
                Handles.Label(transform.position, "No valid position");
            }
        }
#endif
    }
}