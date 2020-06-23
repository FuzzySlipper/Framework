using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class OverheadStrategyController : PlayerController {

        private RtsCameraConfig _camConfig;
        private RtsCameraComponent _rtsCamera;
        private PlayerInputComponent _input;
        private OverheadStrategyInput _strategyInput;
        private OverheadStrategyConfig _config;
        private PlayerCameraComponent _cam;
        
        public RtsCameraComponent RtsCamera { get => _rtsCamera; }
        public PlayerCameraComponent Cam { get => _cam; }

        public OverheadStrategyController(PlayerControllerConfig config) : base(config) {
            _config = (OverheadStrategyConfig) config;
            _camConfig = _config.RtsCameraConfig;
        }

        public override void Enable() {
            base.Enable();
            if (_input != null) {
                PlayerInputSystem.Set(_input);
            }
            if (_rtsCamera != null) {
                _rtsCamera.Active = true;
            }
            UICursor.main.SetDefault();
            RtsCameraSystem.Set(_rtsCamera);
            CameraSystem.Set(_cam);
            _cam.Cam.enabled = true;
            UIMap.main.SetMinimapStatus(false);
            RtsCameraSystem.Get.JumpTo(PlayerPartySystem.Get[0].Tr.position, true);
        }

        public override void Disable() {
            base.Disable();
            if (_input != null) {
                PlayerInputSystem.Remove(_input);
            }
            if (_rtsCamera != null) {
                _rtsCamera.Active = false;
            }
            RtsCameraSystem.Remove(_rtsCamera);
            CameraSystem.Remove(_cam);
            _cam.Cam.enabled = false;
            UIMap.main.SetMinimapStatus(true);
            LazySceneReferences.main.Pathfinding.ClearAllTiles();
        }

        public override void NewGame() {
            base.NewGame();
            MainEntity = Entity.New("StrategyController");
            MainEntity.Add(new TransformComponent(Tr));
            MainEntity.Add(new LabelComponent("StrategyController"));
            // entity.Add(new ImpactRendererComponent(UIPlayerSlot.GetSlot(0)));
            MainEntity.Add(new PlayerRaycastTargeting());
            _strategyInput = new OverheadStrategyInput(LazySceneReferences.main.PlayerInput);
            _input = MainEntity.Add(new PlayerInputComponent(_strategyInput));
            _cam = MainEntity.Add(new PlayerCameraComponent(_config.LookPivot, _config.Cam));
            _rtsCamera = MainEntity.Add(new RtsCameraComponent(_config.Cam, _camConfig));
            _rtsCamera.Active = false;
            UnityToEntityBridge.RegisterToEntity(Tr.gameObject, MainEntity);
        }

        public override void SetActive(bool active) {
            base.SetActive(active);
        }

        public void TurnStart() {
            _strategyInput.TurnStarted();
        }

        public void TurnContinue() {
            _strategyInput.TurnContinue();
        }

        public void TurnEnded() {
            _strategyInput.TurnEnded();
        }

        public override void StartAction(ActionTemplate template) {
            _strategyInput.UseAction(template);
        }
    }
}
