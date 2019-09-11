using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PullTowardsPlayer : MonoBehaviour {

        [SerializeField] private float _force = 5f;
        [SerializeField] private Rigidbody _rb = null;

        public void OnTriggerStay(Collider other) {
            if (other.transform.CompareTag(StringConst.TagPlayer)) {
                _rb.AddForce((other.transform.position - _rb.transform.position) * _force);
            }
        }
    }
}
