using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class OverheadStrategyController : PlayerController {

        private PlayerInputComponent _input;
        private OverheadStrategyConfig _config;
        private PlayerCameraComponent _cam;
        
        public OverheadStrategyController(PlayerControllerConfig config) : base(config) {
            _config = config as OverheadStrategyConfig;
        }

        public override void Enable() {
            base.Enable();
            if (_input != null) {
                PlayerInputSystem.Set(_input);
            }
            CameraSystem.Set(_cam);

        }

        public override void Disable() {
            base.Disable();
            if (_input != null) {
                PlayerInputSystem.Remove(_input);
            }
            CameraSystem.Remove(_cam);
        }

        public override void NewGame() {
            base.NewGame();
            MainEntity = Entity.New("StrategyController");
            MainEntity.Add(new TransformComponent(Tr));
            MainEntity.Add(new LabelComponent("StrategyController"));
            // entity.Add(new ImpactRendererComponent(UIPlayerSlot.GetSlot(0)));
            MainEntity.Add(new PlayerRaycastTargeting());
            _input = MainEntity.Add(new PlayerInputComponent(new OverheadStrategyInput(LazySceneReferences.main.PlayerInput)));
            _cam = MainEntity.Add(new PlayerCameraComponent(_config.LookPivot, null));
            UnityToEntityBridge.RegisterToEntity(Tr.gameObject, MainEntity);
        }
        
        public override void SetActive(bool active) { base.SetActive(active); }
    }
}
