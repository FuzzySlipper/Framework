using UnityEngine;
using System.Collections;
using Rewired;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelComrades {
    public class UISetInputButton : MonoBehaviour {

        [SerializeField] private TextMeshProUGUI _text = null;

        private InputAction _inputAction;
        private ActionElementMap _mapped;
        //private Controller _currentController;
        //private ControllerMap _map;

        void Awake() {
            if (_text== null) {
                _text = GetComponent<TextMeshProUGUI>();
            }
        }

        public void Set(InputAction inputAction, Controller controller) {
            _inputAction = inputAction;
            //_currentController = controller;
            //_map = PlayerInput.RewiredPlayer.controllers.maps.GetMap(controller, 0);
            //_mapped = _map.
            //RefreshLabel();
        }

        public void RefreshLabel() {
            _mapped = PlayerInput.RewiredPlayer.controllers.maps.GetFirstButtonMapWithAction(_inputAction.name, true);
            if (_mapped != null) {
                _text.text = _mapped.elementIdentifierName;
            }
            else {
                _text.text = "Click to bind";
            }
        }

        public void ButtonClicked() {
            if (_inputAction == null) {
                return;
            }
             //= PlayerInput.RewiredPlayer.controllers.maps.GetFirstMapInCategory(_currentController, 0);
            //_text.text = "Press the new button";
            //InputMapper.Context context = new InputMapper.Context() {
            //    actionId = _inputAction.id,
            //    actionRange = AxisRange.Full,
            //    controllerMap = _map,
            //    actionElementMapToReplace = _mapped
            //};
            //UIMainMenu.main.SetInputListen(this, context);
        }

    }
}
