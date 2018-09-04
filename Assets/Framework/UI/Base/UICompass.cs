using UnityEngine;
using System.Collections;
namespace PixelComrades {
    public class UICompass : MonoBehaviour {
        
        [SerializeField] private RectTransform _compassRoot = null;
        [SerializeField] private RectTransform _east = null;
        [SerializeField] private RectTransform _north = null;
        [SerializeField] private RectTransform _south = null;
        [SerializeField] private RectTransform _west = null;

        private int _opposite;
        private int _grade;

        void Update() {
            if (Player.Tr == null) {
                return;
            }
            //return always positive
            _opposite = (int) Mathf.Abs(Player.Tr.eulerAngles.y);
            //never greater than the maximum degree of rotation
            if (_opposite > 360) { //if more
                _opposite = _opposite%360; //return to 0 
            }
            _grade = _opposite;
            //opposite angle
            if (_grade > 180) {
                _grade = _grade - 360;
            }
            _north.anchoredPosition = new Vector2(_compassRoot.sizeDelta.x*0.5f - _grade*2 - _compassRoot.sizeDelta.x*0.5f, 0);
            _south.anchoredPosition =
                new Vector2(_compassRoot.sizeDelta.x*0.5f - _opposite*2 + 360 - _compassRoot.sizeDelta.x*0.5f, 0);
            _east.anchoredPosition = new Vector2(
                _compassRoot.sizeDelta.x*0.5f - _grade*2 + 180 - _compassRoot.sizeDelta.x*0.5f, 0);
            _west.anchoredPosition =
                new Vector2(_compassRoot.sizeDelta.x*0.5f - _opposite*2 + 540 - _compassRoot.sizeDelta.x*0.5f, 0);
        }
    }
}