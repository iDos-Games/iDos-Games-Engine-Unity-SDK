using UnityEngine;

namespace IDosGames
{
	public class PopUpApplicationUpdate : PopUp
	{
		[SerializeField] private PopUpApplicationUpdateView _view;

		public void Set(UpdateUrgency urgency, string version, string linkToUpdate)
		{
			_view.ResetUpdateButton(() => Application.OpenURL(linkToUpdate));
			_view.SetActiveCloseButton(urgency == UpdateUrgency.Optional);
			_view.SetVersionInfo(version);
		}
	}
}