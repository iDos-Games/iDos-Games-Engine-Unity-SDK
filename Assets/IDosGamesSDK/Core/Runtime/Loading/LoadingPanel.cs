using UnityEngine;

namespace IDosGames
{
	public class LoadingPanel : MonoBehaviour
	{
		[SerializeField] private GameObject OpaquePanel;
		[SerializeField] private GameObject TransparentPanel;

		private void Awake()
		{
			HideAllPanels();
		}

		public void HideAllPanels()
		{
			HideOpaquePanel();
			HideTransparentPanel();
		}

		public void Show(LoadingPanelType panelType)
		{
			if (panelType == LoadingPanelType.Opaque)
			{
				ShowOpaquePanel();
				HideTransparentPanel();
			}
			else
			{
				HideOpaquePanel();
				ShowTransparentPanel();
			}
		}

		public void HideOpaquePanel()
		{
			OpaquePanel.SetActive(false);
		}

		public void HideTransparentPanel()
		{
			TransparentPanel.SetActive(false);
		}

		private void ShowOpaquePanel()
		{
			OpaquePanel.SetActive(true);
		}

		private void ShowTransparentPanel()
		{
			TransparentPanel.SetActive(true);
		}
	}
}