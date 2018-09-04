using System;
using UnityEngine;
using System.Collections;
using TMPro;

public class UIDataText : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI _title = null;
    [SerializeField] private TextMeshProUGUI _dataText = null;
    [SerializeField] protected float _textSpeed = 0.1f;

    private int _intData = -1;
    private float _floatData = -1;
    private bool _updatingText = false;

    protected WaitForSeconds Wait;

    public virtual bool Active {get { return _updatingText; } }

    public void SetTitle(string title) {
        if (_title != null) {
            _title.text = title;
        }
    }

    public void UpdateData(int data) {
        if (_updatingText || _intData == data) {
            return;
        }
        _intData = data;
        StartWritingData(data.ToString());
    }

    public void UpdateData(float data) {
        if (_updatingText || Math.Abs(_floatData - data) < 0.1f) {
            return;
        }
        _floatData = data;
        StartWritingData(data.ToString("F1"));
    }

    public virtual void UpdateData(Vector3 data) {}

    public void StartWritingData(string data) {
        _dataText.maxVisibleCharacters = 0;
        _dataText.text = data;
        StartCoroutine(UpdateText(data.Length));
    }

    private IEnumerator UpdateText(int length) {
        _updatingText = true;
        Wait = new WaitForSeconds(_textSpeed);
        while (length > _dataText.maxVisibleCharacters) {
            _dataText.maxVisibleCharacters++;
            yield return Wait;
        }
        _updatingText = false;
    }
}
