﻿using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleFileBrowser
{
#pragma warning disable 649
    public class FileBrowserMovement : MonoBehaviour
	{
		#region Variables
		[SerializeField]
		private RectTransform canvas;

		[SerializeField]
		private RectTransform window;

		[SerializeField]
		private RectTransform dragGizmo;

		[SerializeField]
		private FileBrowser fileBrowser;

		[SerializeField]
		private RecycledListView listView;

		private Vector2 movePointerOffset = Vector2.zero;
		#endregion

		#region Pointer Events
		public void OnDragStarted( BaseEventData data )
		{
			PointerEventData pointer = (PointerEventData) data;

			movePointerOffset = (Vector2) window.position - pointer.position;
		}

		public void OnDrag( BaseEventData data )
		{
			PointerEventData pointer = (PointerEventData) data;

			window.position = pointer.position + movePointerOffset;
		}

		public void OnResize( BaseEventData data )
		{
			PointerEventData pointer = (PointerEventData) data;

			Vector2 deltaSize = pointer.position - (Vector2) dragGizmo.position;
			deltaSize.y = -deltaSize.y;

			Vector2 newSize = window.sizeDelta + deltaSize / canvas.localScale.x;

			if( newSize.x < fileBrowser.MinWidth ) newSize.x = fileBrowser.MinWidth;
			if( newSize.y < fileBrowser.MinHeight ) newSize.y = fileBrowser.MinHeight;

			newSize.x = (int) newSize.x;
			newSize.y = (int) newSize.y;

			deltaSize = newSize - window.sizeDelta;
			deltaSize.y = -deltaSize.y;

			window.sizeDelta = newSize;
			window.anchoredPosition += deltaSize * 0.5f;

			listView.OnViewportDimensionsChanged();
		}
		#endregion
	}
#pragma warning restore 649
}