using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace PixelComrades {
    public class TravelMapConfig : PlayerControllerConfig {
        
        [SerializeField] private Camera _cam = null;
        [SerializeField] private LineRenderer _pathRenderer = null;
        [SerializeField] private HexMoverUnit _playerUnit = null;
        [SerializeField] private Vector3 _lineOffset = Vector3.up;
        [SerializeField] private GameObject[] _activeObjects = new GameObject[0];
        [SerializeField] private float _maxMovesTurn = 10;
        [SerializeField] private GameObjectReference _enemyMoverPrefab = null;
        [SerializeField] private int _maxChasePlayer = 5;
        [SerializeField] private int _enemiesPerFaction = 5;
        [SerializeField] private float _enemyVisibilitySpeed = 0.75f;
        [SerializeField] private TilemapRenderer[] _tileGridRenderers = new TilemapRenderer[2];
        [SerializeField] private RtsCameraConfig _rtsCameraConfig = null;
        
        public RtsCameraConfig RtsCameraConfig { get => _rtsCameraConfig; }
        public Camera Cam { get => _cam; }
        public LineRenderer PathRenderer { get => _pathRenderer; }
        public HexMoverUnit PlayerUnit { get => _playerUnit; }
        public Vector3 LineOffset { get => _lineOffset; }
        public GameObject[] ActiveObjects { get => _activeObjects; }
        public float MaxMovesTurn { get => _maxMovesTurn; }
        public GameObjectReference EnemyMoverPrefab { get => _enemyMoverPrefab; }
        public int MaxChasePlayer { get => _maxChasePlayer; }
        public int EnemiesPerFaction { get => _enemiesPerFaction; }
        public float EnemyVisibilitySpeed { get => _enemyVisibilitySpeed; }
        public TilemapRenderer[] TileGridRenderers { get => _tileGridRenderers; }
    }
}
