using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SimpleFileBrowser
{
	public class FileBrowserQuickLink : FileBrowserItem, IPointerClickHandler
	{
		private string m_targetPath;
		public string TargetPath { get { return m_targetPath; } }

		public void SetQuickLink( Sprite icon, string name, string targetPath )
		{
			SetFile( icon, name, true );

			m_targetPath = targetPath;
		}

		public new void OnPointerClick( PointerEventData eventData )
		{
			fileBrowser.OnQuickLinkSelected( this );
		}
	}
}