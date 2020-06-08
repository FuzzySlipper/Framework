using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PlayerGridControllerConfig : PlayerControllerConfig {
        [SerializeField] private ActionMoveConfig _moveConfig = null;
        [SerializeField] private float _envOffset = 1.75f;
        public ActionMoveConfig MoveConfig { get => _moveConfig; }
        public float EnvOffset { get => _envOffset; }
    }
}
