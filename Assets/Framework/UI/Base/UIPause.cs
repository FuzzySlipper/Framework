using UnityEngine;
using System.Collections;
using PixelComrades;
using UnityEngine.UI;

public class UIPause : MonoBehaviour {

    [SerializeField] private Image _icon = null;
    [SerializeField] private Color _turnBased = Color.cyan;
    [SerializeField] private Color _realTime = Color.green;

    private bool _paused = false;

    void Awake() {
        MessageKit.addObserver(Messages.PauseChanged, CheckPause);
    }
    
    public void TogglePause() {
        if (_paused) {
            Game.RemovePause("TogglePause");
        }
        else {
            Game.Pause("TogglePause");
        }
        _paused = !_paused;
    }

    private void CheckPause() {
        _icon.color = Game.Paused ? _turnBased : _realTime;
    }
}