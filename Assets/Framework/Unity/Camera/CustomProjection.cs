using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    // This script is meant to be attached to your main camera.
// If you want to use it on more than one camera at a time, it will require
// modifcations due to the Camera.on* delegates in OnEnable()/OnDisable().

    [ExecuteInEditMode, RequireComponent(typeof(Camera))]
    public class CustomProjection : MonoBehaviour {

        public Vector4 up = new Vector4(0, 1, 0, 0);
        [SerializeField] private bool _debug = false;

        private Camera _cam;
        private Camera Cam {
            get {
                if (_cam == null) {
                    _cam = GetComponent<Camera>();
                    // Set the camera this script is attached to to use orthographic sorting order.
                    // Instead of using the euclidean distance from the camera, objects will be sorted based on their depth into the scene.
                }
                return _cam;
            }
            set {
                _cam = value;
            }
        }

        private void Update() {
            if (_debug && Input.GetMouseButtonUp(0)) {
                Debug.Log(ScreenToWorldPoint(Input.mousePosition));
            }
        }

	    private void OnEnable(){
		    // Optional, only enable the callbacks when in the editor.
		    //if(Application.isEditor){
			   // // These callbacks are invoked for all cameras including the scene view and camera previews.
			   // Camera.onPreCull += ScenePreCull;
			   // Camera.onPostRender += ScenePostRender;
		    //}
	        Cam.transparencySortMode = TransparencySortMode.Orthographic;
	    }

	    private void OnDisable(){
		    //if(Application.isEditor){
			   // Camera.onPreCull -= ScenePreCull;
			   // Camera.onPostRender -= ScenePostRender;
		    //}
		    Cam.ResetWorldToCameraMatrix();
	    }

	    private void ScenePreCull(Camera cam){
		    // If the camera is the scene view camera, call our OnPreCull() event method for it.
	        if (cam.cameraType == CameraType.SceneView) {
	            OnPreCull();
	        }
	    }

	    private void ScenePostRender(Camera cam){
		    // Unity's gizmos don't like it when you change the worldToCameraMatrix.
		    // The workaround is to reset it after rendering.
	        if (cam.cameraType == CameraType.SceneView) {
	            cam.ResetWorldToCameraMatrix();
	        }
	    }

	    // This is a Unity callback and is the ideal place to set the worldToCameraMatrix.
	    private void OnPreCull(){
		    //var cam = Camera.current;
	        var cam = Cam;
		    // First calculate the regular worldToCameraMatrix.
		    // Start with transform.worldToLocalMatrix.
		    var m = cam.transform.worldToLocalMatrix;

		    // Then, since Unity uses OpenGL's view matrix conventions we have to flip the output z-value.
		    m.SetRow(2, -m.GetRow(2));

		    // Now for the custom projection.
		    // Set the world's up vector to always align with the camera's up vector.
		    // Add a small amount of the original up vector to ensure the matrix will be invertible.
		    m.SetColumn(2, 1e-3f*m.GetColumn(2) - up);

		    cam.worldToCameraMatrix = m;
	    }

	    public static Matrix4x4 ScreenToWorldMatrix(Camera cam){
		    // Make a matrix that converts from screen coordinates to clip coordinates.
		    var rect = cam.pixelRect;
		    var viewportMatrix = Matrix4x4.Ortho(rect.xMin, rect.xMax, rect.yMin, rect.yMax, -1, 1);

		    // The camera's view-projection matrix converts from world coordinates to clip coordinates.
		    var vpMatrix = cam.projectionMatrix*cam.worldToCameraMatrix;

		    // Setting column 2 (z-axis) to identity makes the matrix ignore the z-axis.
		    // Instead you get the value on the xy plane.
		    vpMatrix.SetColumn(2, new Vector4(0, 0, 1, 0));

		    // Going from right to left:
		    // convert screen coords to clip coords, then clip coords to world coords.
		    return vpMatrix.inverse*viewportMatrix;
	    }

	    public Vector2 ScreenToWorldPoint(Vector2 point){
		    return ScreenToWorldMatrix(Cam).MultiplyPoint(point);
	    }

    }
}
