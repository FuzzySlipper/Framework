using UnityEngine;

[ExecuteInEditMode]
public class UIForceDirection : MonoBehaviour {

    [Tooltip("This will force this particular element to always have the specified absolute Z rotation. Use 0 for a straight upwards facing.")]
    [SerializeField] private float _forcedZRotation = 0f;
    private Vector3 _rot = Vector3.zero;
    private RectTransform _rt;

    private void Awake() {
        _rot.z = _forcedZRotation;
        _rt = GetComponent<RectTransform>();
    }
    
    private void Update() {
        if (!Application.isPlaying) {
            _rot.z = _forcedZRotation;
        }

        if (_rt.eulerAngles != _rot) {
            _rt.eulerAngles = _rot;
        }
    }
}