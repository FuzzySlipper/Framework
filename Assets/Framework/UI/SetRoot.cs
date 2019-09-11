using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class SetRoot : MonoBehaviour {
        [SerializeField] private UIRoot _target = UIRoot.Debug;

        void Awake() {
            Root.Register(_target, GetComponent<Canvas>());
        }
    }
}