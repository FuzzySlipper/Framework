using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades.DungeonCrawler;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class TestFlightComponents : MonoBehaviour {

        [SerializeField] private FlightEngineConfig _engineConfig = null;
        [SerializeField] private FlightControlConfig _controlConfig = null;
        [SerializeField] private FakeFlightEngineConfig _fakeFlightConfig = null;
        [SerializeField] private Transform _bankTransform = null;
        [SerializeField] private FloatRange _shootCooldown = new FloatRange(0.25f, 0.5f);
        [SerializeField] private float _damage = 20f;
        [SerializeField] private bool _createEntity = true;

        [Header("Status")] 
        [SerializeField, Range(-1, 1)] private float _pitch;
        [SerializeField, Range(-1, 1)] private float _yaw;
        [SerializeField, Range(-1, 1)] private float _roll;
        [SerializeField, Range(-1, 1)] private float _strafeHorizontal;
        [SerializeField, Range(-1, 1)] private float _strafeVertical;
        [SerializeField, Range(-1, 1)] private float _thrust;
        [SerializeField, Range(-1, 1)] private float _boost;
        [SerializeField] private float _speed;
        [SerializeField] private string _debugString = "";

        private Entity _entity;
        private FlightEngine _flightEngine;
        private FlightControl _flightControl;

        void Awake() {
            World.Get<FlightPhysicsSystem>();
            World.Get<FlightNpcSystem>();
            if (_createEntity) {
                TimeManager.PauseFor(0.1f, true, () => {
                    CreateEntity();
                    RiftMapGenerator.main.Generate();
                    var node = RiftMap.Random();
                    RiftMapNode neighbor = null;
                    for (int i = 0; i < node.Neighbors.Length; i++) {
                        if (node.Neighbors[i] != null) {
                            neighbor = node.Neighbors[i];
                            break;
                        }
                    }
                    RiftMap.UpdateCurrent(neighbor);
                    RiftSpaceTunnelController.Activate(node);
                });
            }
        }

        private void CreateEntity() {
            _entity = Entity.New("TestFlight");
            _entity.Add(new TransformComponent(transform));
            _flightControl = _entity.Add(new FlightControl(_controlConfig));
            _flightEngine = _entity.Add(new FlightEngine(_engineConfig));
            _entity.Add(new FakeFlightEngine(_fakeFlightConfig));
            _entity.Add(new CosmeticFlightBanking(_bankTransform, _controlConfig));
            _entity.Add(new RigidbodyComponent(GetComponent<Rigidbody>()));
            _entity.Add(new PlayerInputComponent(PlayerInput.main));
            var dmgStat = new BaseStat(_entity,_damage, "Damage", 9999);
            var stats = _entity.Add(new StatsContainer());
            stats.Add(dmgStat);
            _entity.Add(
                new SimpleProjectileSpawner(
                    _bankTransform, "SpaceLaser", _shootCooldown, new List<IActionImpact>() {
                        new DamageImpact( "DamageTypes.Physical", "Vitals.Health", 1, dmgStat)
                    }));
            _entity.Add(new FlightPlayerInput(PlayerInput.main, "Sprint", "FlightThrust", "FlightStrafeHorizontal", "FlightStrafeVertical", "FlightPitch", "FlightYaw", "FlightRoll"));
        }

        public void SetEntity(Entity entity) {
            _entity = entity;
            _flightControl = _entity.Get<FlightControl>();
            _flightEngine = _entity.Get<FlightEngine>();
        }

        void Update() {
            if (_flightControl == null) {
                return;
            }
            Cursor.lockState = !_controlConfig.UseDirectControl? CursorLockMode.None : CursorLockMode.Locked;
            _pitch = _flightControl.Pitch;
            _yaw = _flightControl.Yaw;
            _roll = _flightControl.Roll;
            _strafeHorizontal = _flightControl.StrafeHorizontal;
            _strafeVertical = _flightControl.StrafeVertical;
            _thrust = _flightControl.Thrust;
            _boost = _flightControl.Boost;
            _speed = _flightControl.Speed;
            var node = _entity.GetNode<FlyingNode>();
            if (node == null) {
                _debugString = "No Node";
                return;
            }
            _debugString = string.Format("Mode {0}", node.Control.CurrentMode);
            if (Input.GetMouseButtonDown(0)) {
                node.Projectile.Fire();
            }
        }

        [Button]
        public void UpdateConfig() {
            if (_flightEngine != null) {
                _flightEngine.RefreshEngine();
            }
        }
    }
}
