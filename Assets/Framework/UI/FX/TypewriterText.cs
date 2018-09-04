using System;
using UnityEngine;
using System.Collections;
using System.Text;

[System.Serializable]
public class TypewriterText {
    public float Delay;
    public AudioClip Clip;
    public AudioSource Source;
    public Action<string> UpdateTextEvent;
    public Action OnComplete;

    private Char[] _characters;
    private StringBuilder _string;
    private WaitForSeconds _wait;

    public bool Active { get; private set; }
    
    public IEnumerator TextTypeCoroutine(string text) {
        _wait = new WaitForSeconds(Delay);
        _string = new StringBuilder();
        _characters = text.ToCharArray();
        Active = true;
        for (int i = 0; i < _characters.Length; i++) {
            _string.Append(_characters[i]);
            if (UpdateTextEvent != null) {
                UpdateTextEvent(_string.ToString());
            }
            if (Clip != null && Source != null) {
                Source.PlayOneShot(Clip);
            }
            yield return _wait;
        }
        if (OnComplete != null) {
            OnComplete();
        }
        Active = false;
    }
}
