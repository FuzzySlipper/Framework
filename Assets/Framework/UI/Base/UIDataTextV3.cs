using System;
using UnityEngine;
using System.Collections;
using TMPro;

public class UIDataTextV3 : UIDataText {
    [SerializeField] private TextMeshProUGUI[] _dataV3Text = new TextMeshProUGUI[3];
    
    private bool[] _updatingTextArray = new bool[3] {false,false,false};
    private float[] _v3Data = new float[3] { float.MaxValue, float.MaxValue, float.MaxValue};

    public override bool Active {
        get {
            for (int i = 0; i < _updatingTextArray.Length; i++) {
                if (_updatingTextArray[i]) {
                    return true;
                }
            }
            return false;
        }
    }

    void Awake() {
        Wait = new WaitForSeconds(_textSpeed);
    }

    public override void UpdateData(Vector3 data) {
        for (int i = 0; i < 3; i++) {
            if (_updatingTextArray[i]) {
                continue;
            }
            if (Math.Abs(_v3Data[i] - data[i]) < 0.1f) {
                continue;
            }
            StartWritingDataV3(data[i].ToString("F1"), i);
            _v3Data[i] = data[i];
        }
    }

    private void StartWritingDataV3(string data, int index) {
        _dataV3Text[index].maxVisibleCharacters = 0;
        _dataV3Text[index].text = data;
        StartCoroutine(UpdateText(data.Length, index));
    }

    private IEnumerator UpdateText(int length, int index) {
        _updatingTextArray[index] = true;
        while (length > _dataV3Text[index].maxVisibleCharacters) {
            _dataV3Text[index].maxVisibleCharacters++;
            yield return Wait;
        }
        _updatingTextArray[index] = false;
    }

}
