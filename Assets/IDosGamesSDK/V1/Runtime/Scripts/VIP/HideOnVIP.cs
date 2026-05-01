using UnityEngine;

namespace IDosGames
{
	public class HideOnVIP : MonoBehaviour
	{
		private void OnEnable()
		{
			UpdateView();
            IDosGamesData.User.OnAnyUpdated += UpdateView;
		}

		private void OnDisable()
		{
            IDosGamesData.User.OnAnyUpdated -= UpdateView;
		}

		private void UpdateView()
		{
			if (DataService.HasVIPStatus)
			{
				gameObject.SetActive(false);
			}
		}
	}
}
