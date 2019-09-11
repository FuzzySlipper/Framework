using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;

namespace PixelComrades.DungeonCrawler {
    public class SpriteHolder : MonoBehaviour {

        [SerializeField] private BillboardMode _billboard = BillboardMode.CamFwd;
        [SerializeField] private bool _unscaled = true;
        [SerializeField] private SpriteRenderer _renderer = null;
        [SerializeField] private Transform _spriteBaseTr = null;
        [SerializeField] private Transform _spriteTr = null;
        [SerializeField] private SpriteFacing _facing = SpriteFacing.EightwayFlipped;
        [SerializeField] private bool _backwards = true;
        [SerializeField] private List<Collider> _ignoreColliders = new List<Collider>();
        [SerializeField] private KinematicCharacterMotor _motor = null;

        public BillboardMode Billboard { get => _billboard; }
        public bool Unscaled { get => _unscaled; }
        public SpriteRenderer Renderer { get => _renderer; }
        public Transform SpriteBaseTr { get => _spriteBaseTr; }
        public Transform SpriteTr { get => _spriteTr; }
        public SpriteFacing Facing { get => _facing; }
        public bool Backwards { get => _backwards; }
        public List<Collider> IgnoreColliders { get => _ignoreColliders; }
        public KinematicCharacterMotor Motor { get => _motor; }
    }
}
