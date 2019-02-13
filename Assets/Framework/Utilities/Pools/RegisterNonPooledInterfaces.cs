using UnityEngine;
using System.Collections;
using PixelComrades;

public class RegisterNonPooledInterfaces : MonoBehaviour {
    private ISystemFixedUpdate[] _systemFixedUpdate;
    private ISystemUpdate[] _systemUpdate;
    private IPoolEvents[] _poolListeners;
    private ITurnUpdate[] _turnUpdate;

    public void Awake() {
        _systemFixedUpdate = GetComponentsInChildren<ISystemFixedUpdate>(true);
        _systemUpdate = GetComponentsInChildren<ISystemUpdate>(true);
        _poolListeners = GetComponentsInChildren<IPoolEvents>(true);
        _turnUpdate = GetComponentsInChildren<ITurnUpdate>(true);
    }

    void OnEnable() {
        SetActive(true);
    }

    void OnDisable() {
        SetActive(false);
    }

    
    public virtual void SetActive(bool status) {
        gameObject.SetActive(status);
        if (!Application.isPlaying) {
            return;
        }
        if (_poolListeners != null) {
            for (int i = 0; i < _poolListeners.Length; i++) {
                if (_poolListeners[i]== null) {
                    continue;
                }
                if (status) {
                    _poolListeners[i].OnPoolSpawned();
                }
                else {
                    _poolListeners[i].OnPoolDespawned();
                }
            }
        }
        if (_systemFixedUpdate != null) {
            for (int i = 0; i < _systemFixedUpdate.Length; i++) {
                if (_systemFixedUpdate[i] == null) {
                    continue;
                }
                if (status) {
                    SystemManager.AddFixed(_systemFixedUpdate[i]);
                }
                else {
                   SystemManager.Remove(_systemFixedUpdate[i]);
                }
            }
        }
        if (_systemUpdate != null) {
            for (int i = 0; i < _systemUpdate.Length; i++) {
                if (_systemUpdate[i] == null) {
                    continue;
                }
                if (status) {
                    SystemManager.Add(_systemUpdate[i]);
                }
                else {
                   SystemManager.Remove(_systemUpdate[i]);
                }
            }
        }
        if (_turnUpdate != null) {
            for (int i = 0; i < _turnUpdate.Length; i++) {
                if (_turnUpdate[i] == null) {
                    continue;
                }
                if (status) {
                    SystemManager.AddTurn(_turnUpdate[i]);
                }
                else {
                   SystemManager.RemoveTurn(_turnUpdate[i]);
                }
            }
        }
    }

}