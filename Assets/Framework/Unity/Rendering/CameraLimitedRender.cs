using UnityEngine;
using System.Collections;

public class CameraLimitedRender : MonoBehaviour {
    [SerializeField] private GameObject _limitedRender = null;

    private MeshRenderer[] _renderers;

    void Awake() {
        _renderers = _limitedRender.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < _renderers.Length; i++) {
            _renderers[i].enabled = false;
        }
    }

    void OnPreCull() {
        for (int i = 0; i < _renderers.Length; i++) {
            _renderers[i].enabled = true;
        }
    }

    void OnPostRender(){
        for (int i = 0; i < _renderers.Length; i++) {
            _renderers[i].enabled = false;
        }
    }
}