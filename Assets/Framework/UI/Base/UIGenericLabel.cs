using UnityEngine;
using System.Collections;
using TMPro;

public class UIGenericLabel : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _text = null;

    public void SetText(string text) {
        _text.text = text;
    }

}