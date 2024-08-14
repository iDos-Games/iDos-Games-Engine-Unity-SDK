using UnityEngine;

namespace IDosGames
{
	public class SkinItemMenu : MonoBehaviour
	{
		[SerializeField] private SkinItemMenuPopUp _popUp;

		public void ShowPopUp(Transform transform, SkinCatalogItem item)
		{
			_popUp.SetButtons(item);
			_popUp.SetPosition(transform);
			gameObject.SetActive(true);
		}
	}
}
