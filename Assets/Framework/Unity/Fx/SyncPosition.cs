using UnityEngine;
using System.Collections;

public class SyncPosition : MonoBehaviour {
    [SerializeField] private Transform _target = null;
    [SerializeField] private Vector3 _offset = Vector3.zero;

    void Update() {
        if (_target != null) {
            transform.position = _target.position + _offset;
        }
    }
}
