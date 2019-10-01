﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SimpleFileBrowser
{
#pragma warning disable 649
	public class FileBrowserItem : ListItem, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		#region Constants
		private const float DOUBLE_CLICK_TIME = 0.5f;
		#endregion

		#region Variables
		protected FileBrowser fileBrowser;

		[SerializeField]
		private Image background;

		[SerializeField]
		private Image icon;

		[SerializeField]
		private TextMeshProUGUI nameText;

		private float prevTouchTime = Mathf.NegativeInfinity;
		#endregion

		#region Properties
		private RectTransform m_transform;
		public RectTransform TransformComponent
		{
			get
			{
				if( m_transform == null )
					m_transform = (RectTransform) transform;

				return m_transform;
			}
		}

		public string Name { get { return nameText.text; } }

		private bool m_isDirectory;
		public bool IsDirectory { get { return m_isDirectory; } }
		#endregion

		#region Initialization Functions
		public void SetFileBrowser( FileBrowser fileBrowser )
		{
			this.fileBrowser = fileBrowser;
		}

		public void SetFile( Sprite icon, string name, bool isDirectory )
		{
			this.icon.overrideSprite = icon;
			nameText.text = name;

			m_isDirectory = isDirectory;
		}
		#endregion

		#region Pointer Events
		public void OnPointerClick( PointerEventData eventData )
		{
			if( Time.realtimeSinceStartup - prevTouchTime < DOUBLE_CLICK_TIME )
			{
				if( fileBrowser.SelectedFilePosition == Position )
					fileBrowser.OnItemOpened( this );

				prevTouchTime = Mathf.NegativeInfinity;
			}
			else
			{
				fileBrowser.OnItemSelected( this );
				prevTouchTime = Time.realtimeSinceStartup;
			}
		}

		public void OnPointerEnter( PointerEventData eventData )
		{
#if UNITY_EDITOR || ( !UNITY_ANDROID && !UNITY_IOS )
			if( fileBrowser.SelectedFilePosition != Position )
				background.color = fileBrowser.hoveredFileColor;
#endif
		}

		public void OnPointerExit( PointerEventData eventData )
		{
#if UNITY_EDITOR || ( !UNITY_ANDROID && !UNITY_IOS )
			if( fileBrowser.SelectedFilePosition != Position )
				background.color = fileBrowser.normalFileColor;
#endif
		}
		#endregion

		#region Other Events
		public void Select()
		{
			background.color = fileBrowser.selectedFileColor;
		}

		public void Deselect()
		{
			background.color = fileBrowser.normalFileColor;
		}

		public void SetHidden( bool isHidden )
		{
			Color c = icon.color;
			c.a = isHidden ? 0.5f : 1f;
			icon.color = c;

			c = nameText.color;
			c.a = isHidden ? 0.55f : 1f;
			nameText.color = c;
		}
		#endregion
	}
#pragma warning restore 649
}