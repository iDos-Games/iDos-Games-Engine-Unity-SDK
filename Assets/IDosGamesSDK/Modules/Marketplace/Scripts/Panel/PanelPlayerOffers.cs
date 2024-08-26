using UnityEngine;

namespace IDosGames
{
	public class PanelPlayerOffers : MonoBehaviour
	{
		[SerializeField] private PanelPlayerActiveOffers _activeOffersPanel;
		public PanelPlayerActiveOffers ActiveOffersPanel => _activeOffersPanel;

		[SerializeField] private PanelPlayerHistory _historyPanel;
		public PanelPlayerHistory HistoryPanel => _historyPanel;

		[SerializeField] private TabItem _activeOffersTab;
		[SerializeField] private TabItem _historyTab;

#if IDOSGAMES_MARKETPLACE
		private void OnEnable()
		{
			OpenActiveOffersPanel();
		}

		public void OpenActiveOffersPanel()
		{
			_historyPanel.gameObject.SetActive(false);
			_activeOffersPanel.gameObject.SetActive(true);

			_historyTab.Deselect();
			_activeOffersTab.Select();
		}

		public void OpenHistoryOffersPanel()
		{
			_activeOffersPanel.gameObject.SetActive(false);
			_historyPanel.gameObject.SetActive(true);

			_activeOffersTab.Deselect();
			_historyTab.Select();
		}

		public void Refresh()
		{
			if (_activeOffersPanel.gameObject.activeSelf)
			{
				_activeOffersPanel.Refresh();
			}
			else if (_historyPanel.gameObject.activeSelf)
			{
				_historyPanel.Refresh();
			}
		}
#endif

    }
}
