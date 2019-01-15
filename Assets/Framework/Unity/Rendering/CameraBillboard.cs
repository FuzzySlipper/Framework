using UnityEngine;
using System.Collections;
namespace PixelComrades {
    [ExecuteInEditMode]
    public class CameraBillboard : MonoBehaviour {

        [SerializeField] private bool _backwards = false;
        [SerializeField] private BillboardMode _billboard = BillboardMode.NoYAxis;

        public bool Backwards { get => _backwards; set => _backwards = value; }

        void Update() {
            _billboard.Apply(transform, _backwards);
            //var lookPos = transform.position + Player.Camera.transform.rotation * Vector3.forward;
            //if (_forceUp) {
            //    transform.LookAt(_backwards ? lookPos : -lookPos, Player.Camera.transform.rotation * Vector3.up);
            //}
            //else {
            //    transform.LookAt(_backwards ? lookPos : -lookPos,transform.up);
            //}
        }
    }
}   