using System;
using UnityEngine;

namespace IDosGames
{
	public class ActiveOfferMenu : MonoBehaviour
	{
		[SerializeField] private ActiveOfferMenuPopUp _popUp;
		[SerializeField] private MarketplaceWindow _marketplaceWindow;

#if IDOSGAMES_MARKETPLACE
		public void ShowPopUp(Transform transform, string itemID, Action editAction, Action deleteAction)
		{
			_popUp.SetButtons(() => InspectSkin(itemID), editAction, deleteAction);
			_popUp.SetPosition(transform);
			SetActivateMenu(true);
		}

		private void InspectSkin(string itemID)
		{
			_marketplaceWindow.InspectSkin(itemID);
		}

		public void SetActivateMenu(bool active)
		{
			gameObject.SetActive(active);
		}
#endif

    }
}
