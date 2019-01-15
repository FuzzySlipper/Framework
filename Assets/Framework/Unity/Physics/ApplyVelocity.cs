using UnityEngine;
using System.Collections;


public class ApplyVelocity : MonoBehaviour {
    [SerializeField] private Rigidbody[] _rigidbodies = new Rigidbody[0];

    public void Apply(Vector3 velocity, Vector3 pos) {
        for (int i = 0; i < _rigidbodies.Length; i++) {
            //_rigidbodies[i].AddExplosionForce(impact, pos, impact * 8, 3);
            //_rigidbodies[i].velocity = velocity;
            _rigidbodies[i].AddForceAtPosition(velocity, pos);
        }
    }

    private void GatherRigidbodies() {
        _rigidbodies = GetComponentsInChildren<Rigidbody>();
    }
}