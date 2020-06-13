using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace PixelComrades {
    public class PlayerGridControllerConfig : PlayerControllerConfig {
        [SerializeField] private ActionMoveConfig _moveConfig = null;
        [SerializeField] private float _envOffset = 1.75f;
        [SerializeField] private AudioClipSet _playerAudioSet = null;
        [SerializeField] private Transform _lookPivot = null;

        public ActionMoveConfig MoveConfig { get => _moveConfig; }
        public float EnvOffset { get => _envOffset; }
        public AudioClipSet PlayerAudioSet { get => _playerAudioSet; }
        public Transform LookPivot { get => _lookPivot; }

    }
}
