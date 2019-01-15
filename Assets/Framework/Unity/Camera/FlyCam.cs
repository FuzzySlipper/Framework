using UnityEngine;
using System.Collections;
namespace PixelComrades {
    public class FlyCam : MonoSingleton<FlyCam> {


        [SerializeField] private float _cameraSensitivity = 90;
        [SerializeField] private float _climbSpeed = 4;
        [SerializeField] private float _normalMoveSpeed = 10;
        [SerializeField] private float _slowMoveFactor = 0.25f;
	    [SerializeField] private float _fastMoveFactor = 3;
 
	    private float _rotationX = 0.0f;
	    private float _rotationY = 0.0f;
        private Vector3 _oldPos;
        private Quaternion _oldRot;
        private bool _isActive = false;

        public bool IsActive { get { return _isActive; } }


	    public void ToggleActive() {
            SetIsActive(!IsActive);
	    }

        public void SetIsActive(bool active) {
            _isActive = active;
            if (IsActive) {
                _oldPos = Player.Cam.transform.localPosition;
                _oldRot = Player.Cam.transform.localRotation;
                PlayerInput.MoveInputLocked = true;
            }
            else {
                Player.Cam.transform.localPosition = _oldPos;
                Player.Cam.transform.localRotation = _oldRot;
                PlayerInput.MoveInputLocked = false;
            }
        }
 
	    void Update () {
	        if (!IsActive) {
	            return;
	        }
            if (Input.GetKeyDown (KeyCode.Z)) {
                Cursor.lockState = Cursor.lockState != CursorLockMode.Locked ? 
                CursorLockMode.Locked : CursorLockMode.None;
		    }
            
		    _rotationX += Input.GetAxis("Mouse X") * _cameraSensitivity * Time.deltaTime;
		    _rotationY += Input.GetAxis("Mouse Y") * _cameraSensitivity * Time.deltaTime;
		    _rotationY = Mathf.Clamp (_rotationY, -90, 90);
 
		    transform.localRotation = Quaternion.AngleAxis(_rotationX, Vector3.up);
		    transform.localRotation *= Quaternion.AngleAxis(_rotationY, Vector3.left);
 
	 	    if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
	 	    {
			    transform.position += transform.forward * (_normalMoveSpeed * _fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
			    transform.position += transform.right * (_normalMoveSpeed * _fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
	 	    }
	 	    else if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl))
	 	    {
			    transform.position += transform.forward * (_normalMoveSpeed * _slowMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
			    transform.position += transform.right * (_normalMoveSpeed * _slowMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
	 	    }
	 	    else
	 	    {
	 		    transform.position += transform.forward * _normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
			    transform.position += transform.right * _normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
	 	    }
 
 
		    if (Input.GetKey (KeyCode.Q)) {transform.position += transform.up * _climbSpeed * Time.deltaTime;}
		    if (Input.GetKey (KeyCode.E)) {transform.position -= transform.up * _climbSpeed * Time.deltaTime;}
 
		    
	    }
    }
}