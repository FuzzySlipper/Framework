using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
	public abstract class GraphSidebar {

		public void DrawSidebar (Rect rect, float padding, Color color, GraphController controller) {
			float innerWidth = rect.width - (padding * 2f);
			float innerHeight = rect.height - (padding * 2f);

			GUI.BeginGroup(rect); // Container
			DrawBox(new Rect(0, 0, rect.width, rect.height), color);
			GUI.BeginGroup(new Rect(padding, padding, innerWidth, innerHeight)); // Padding
            SidebarOptions(innerWidth, innerHeight, controller);
			GUI.EndGroup();
			GUI.EndGroup(); // Container
		}

	    protected abstract void SidebarOptions(float innerWidth, float innerHeight, GraphController controller);

		protected void DrawBox (Rect position, Color color) {
			Color oldColor = GUI.color;

			GUI.color = color;
			GUI.Box(position, "");

			GUI.color = oldColor;
		}
	}
}
