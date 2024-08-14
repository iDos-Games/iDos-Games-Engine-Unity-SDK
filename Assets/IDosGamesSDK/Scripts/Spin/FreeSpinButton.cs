using System;
using UnityEngine;

namespace IDosGames
{
	public class FreeSpinButton : SpinButton
	{
		[SerializeField] private FreeSpinButtonView _view;

		public void Set(Action tryToSpinAction, DateTime endDate, string quantity, bool showAdIcon, bool block)
		{
			ResetButtonListener(tryToSpinAction);

			if (block)
			{
				_view.Block(endDate);
				Button.interactable = false;
			}
			else
			{
				_view.UnBlock();
				Button.interactable = true;
				_view.SetQuantity(quantity);
				_view.SetActiveAddIcon(showAdIcon);
			}
		}

		private void ResetButtonListener(Action action)
		{
			Button.onClick.RemoveAllListeners();
			Button.onClick.AddListener(() => action());
		}
	}
}