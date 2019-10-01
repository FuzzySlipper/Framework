using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class PlayerFlightController : MonoBehaviour, IPlayerFlightController {

        [SerializeField] private FlightEngineConfig _engineConfig = null;
        [SerializeField] private FlightControlConfig _controlConfig = null;
        [SerializeField] private FakeFlightEngineConfig _fakeFlightConfig = null;
        [SerializeField] private Transform _bankTransform = null;
        [SerializeField] private Transform _firePivot = null;
        [SerializeField] private NormalizedFloatRange _shootCooldown = new NormalizedFloatRange(0.25f, 0.5f);
        [SerializeField] private float _damage = 20f;
        [SerializeField] private Collider _collider = null;
        [SerializeField] private Camera _camera = null;

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
        private Rigidbody _rb;
        public Entity Entity { get => _entity; }
        public FakeFlightEngineConfig FakeFlightConfig { get => _fakeFlightConfig; }
        public FlightEngine FlightEngine { get => _flightEngine; }
        public FlightControl FlightControl { get => _flightControl; }
        public Rigidbody Rb { get => _rb; }
        public Transform Tr { get; private set;}
        public Collider Collider { get => _collider; }
        public float Thrust => _flightControl.Thrust;

        void Awake() {
            gameObject.SetActive(false);
            Player.Flight = this;
        }

        public void SetStatus(bool status) {
            gameObject.SetActive(status);
            Player.Controller.SetActive(!status);
        }

        public void NewGame() {
            _entity = Entity.New("Ship");
            Tr = transform;
            _entity.Add(new TransformComponent(Tr));
            _rb = GetComponent<Rigidbody>();
            _flightControl = _entity.Add(new FlightControl(_controlConfig));
            _flightEngine = _entity.Add(new FlightEngine(_engineConfig));
            _entity.Add(new FakeFlightEngine(_fakeFlightConfig));
            _entity.Add(new ColliderComponent(_collider));
            _entity.Add(new CosmeticFlightBanking(_bankTransform, _controlConfig));
            _entity.Add(new RigidbodyComponent(_rb));
            var dmgStat = new BaseStat(_entity, _damage, "Damage", 9999);
            var stats = _entity.Add(new StatsContainer());
            stats.Add(dmgStat);
            _entity.Add(new DamageImpact("DamageTypes.Physical", "Vitals.Health", 1));
            _entity.Add(new SimpleProjectileSpawner(_firePivot, "SpaceLaser", _shootCooldown));
            _entity.Add(new FlightPlayerInput(_entity.Get<PlayerInputComponent>(), "Sprint", PlayerFlightControls.FlightThrust, PlayerFlightControls
            .FlightStrafeHorizontal, PlayerFlightControls.FlightStrafeVertical, PlayerFlightControls.FlightPitch, PlayerFlightControls.FlightYaw, PlayerFlightControls.FlightRoll));
            UnityToEntityBridge.RegisterToEntity(gameObject, _entity);
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
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                node.Projectile.Fire(ray.origin + (ray.direction * 1000));
            }
        }

        [Button]
        public void UpdateConfig() {
            if (_flightEngine != null) {
                _flightEngine.RefreshEngine();
            }
        }

        public void SetMode(FlightControl.Mode mode) {
            _flightControl.SetMode(mode);
        }
        public void SetDefaultMode() {
            _flightControl.SetMode();
        }
    }

    public static class PlayerFlightControls {
        public const string FlightThrust = "FlightThrust";
        public const string FlightStrafeHorizontal = "FlightStrafeHorizontal";
        public const string FlightStrafeVertical = "FlightStrafeVertical";
        public const string FlightYaw = "FlightYaw";
        public const string FlightPitch = "FlightPitch";
        public const string FlightRoll = "FlightRoll";
    }
}
