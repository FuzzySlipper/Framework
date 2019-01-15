using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class SetRoot : MonoBehaviour {
        [SerializeField] private UIRoot _target;

        void Awake() {
            Root.Register(_target, GetComponent<Canvas>());
        }
    }
}