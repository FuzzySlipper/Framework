using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
	public class GraphCamera {
		public Vector2 Offset; // Total offset tracked by the camera
		public bool Move; // Is the camera moving?
		public Vector2 ViewportSize;

	    public GraphCamera(Vector2 viewPortSize) {
	        ViewportSize = viewPortSize;
            Reset();
        }

		private Vector2 _prevPos; // Used to track position changes
		private bool _debug = false;

		public void BeginMove (Vector2 startPos) {
			Move = true;
			_prevPos = startPos;
		    if (_debug) {
		        Debug.Log("Begin camera move");
		    }
		}

		public void EndMove () {
			Move = false;
		    if (_debug) {
		        Debug.Log("End camera move");
		    }
		}

		public bool PollCamera (Vector2 newPos) {
		    if (!Move) {
		        return false;
		    }
			Offset += _prevPos - newPos;
			_prevPos = newPos;
			return true;
		}

		public Vector2 GetMouseGlobal (Vector2 mouse) {
			return new Vector2(mouse.x + Offset.x - (ViewportSize.x / 2f), mouse.y + Offset.y - (ViewportSize.y / 2f));
		}

		public Vector2 GetOffsetGlobal () {
			return new Vector2(Offset.x - (ViewportSize.x / 2f), Offset.y - (ViewportSize.y / 2f));
		}

		public void Reset () {
			Move = false;
		    //Offset = new Vector2(ViewportSize0.5f, 0.5f);
		    Offset = ViewportSize * 0.25f;
		    if (_debug) {
		        Debug.Log("Camera reset");
		    }
		}
	}
}
