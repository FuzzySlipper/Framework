using UnityEngine;
using System.Collections;

public class SetRoot : MonoBehaviour {
    public enum TargetRoot {
        Canvas,
        CanvasMisc,
        Player,
    }

    [SerializeField] private TargetRoot _target;

    void Awake() {
        switch (_target) {
            case TargetRoot.Canvas:
                Root.Canvas = GetComponent<Canvas>();
                break;
            case TargetRoot.CanvasMisc:
                Root.CanvasMisc = GetComponent<Canvas>();
                break;
            case TargetRoot.Player:
                Root.PlayerCanvas = GetComponent<Canvas>();
                break;
        }
    }
}