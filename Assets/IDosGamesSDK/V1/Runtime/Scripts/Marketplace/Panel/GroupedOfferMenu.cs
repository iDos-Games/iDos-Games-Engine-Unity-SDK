
using UnityEngine;

namespace IDosGames
{
	public class GroupedOfferMenu : MonoBehaviour
	{
		[SerializeField] private GroupedOfferMenuPopUp _popUp;

#if IDOSGAMES_MARKETPLACE
		public void ShowPopUp(Transform transform, string itemID)
		{
			_popUp.SetButtons(itemID);
			_popUp.SetPosition(transform);
			SetActivateMenu(true);
		}

		public void SetActivateMenu(bool active)
		{
			gameObject.SetActive(active);
		}
#endif

    }
}
