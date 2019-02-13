using UnityEngine;
using System.Collections;

[System.Serializable]
public class GuidString {
    [SerializeField] private string _id = "";

    public string Id {
        get {
            if (string.IsNullOrEmpty(_id)) { _id = System.Guid.NewGuid().ToString();}
            return _id;
        }
    }

}