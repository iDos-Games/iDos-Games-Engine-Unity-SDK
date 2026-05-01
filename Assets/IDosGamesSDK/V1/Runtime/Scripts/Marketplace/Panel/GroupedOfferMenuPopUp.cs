using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class GroupedOfferMenuPopUp : MonoBehaviour
	{
		[SerializeField] private GroupedOfferMenu _groupedOfferMenu;
		[SerializeField] private MarketplaceWindow _marketplaceWindow;
		[SerializeField] private Button _buyButton;
		[SerializeField] private Button _inspectButton;

#if IDOSGAMES_MARKETPLACE
		public void SetPosition(Transform target)
		{
			transform.position = target.position;
		}

		public void SetButtons(string itemID)
		{
			SetInspectButton(itemID);
			SetBuyButton(itemID);
		}

		private void SetBuyButton(string itemID)
		{
			_buyButton.onClick.RemoveAllListeners();
			_buyButton.onClick.AddListener(async () => await _marketplaceWindow.OpenBuySkinByItemID(itemID));
			_buyButton.onClick.AddListener(() => _groupedOfferMenu.SetActivateMenu(false));
		}

		private void SetInspectButton(string itemID)
		{
			_inspectButton.onClick.RemoveAllListeners();
			_inspectButton.onClick.AddListener(() => _marketplaceWindow.InspectSkin(itemID));
			_inspectButton.onClick.AddListener(() => _groupedOfferMenu.SetActivateMenu(false));
		}
#endif

    }
}
