using UnityEngine;
using System.Collections;
namespace PixelComrades {
    [ExecuteInEditMode]
    public class CameraBillboard : MonoBehaviour {

        [SerializeField] private bool _backwards = false;
        [SerializeField] private BillboardMode _billboard = BillboardMode.NoYAxis;

        public bool Backwards { get => _backwards; set => _backwards = value; }
        private float _lastAngleHeight;

        void Update() {
            _billboard.Apply(transform, _backwards, ref _lastAngleHeight);
            //var lookPos = transform.position + Player.Camera.transform.rotation * Vector3.forward;
            //if (_forceUp) {
            //    transform.LookAt(_backwards ? lookPos : -lookPos, Player.Camera.transform.rotation * Vector3.up);
            //}
            //else {
            //    transform.LookAt(_backwards ? lookPos : -lookPos,transform.up);
            //}
        }

        #if UNITY_EDITOR

        

        void OnDrawGizmosSelected() {
            Game.SpriteCamera = Camera.current;
            _billboard.Apply(transform, _backwards, ref _lastAngleHeight);
        }
        #endif
    }
}   