using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
	public class ChestKeyFragmentBar : CurrencyBar
	{
		[SerializeField] private ChestKeyFragmentType _fragmentType;

		private void OnEnable()
		{
			UpdateAmount();
			IDosGamesData.User.OnInventoryUpdated += UpdateAmount;
		}

		private void OnDisable()
		{
			IDosGamesData.User.OnInventoryUpdated -= UpdateAmount;
		}

		public override void UpdateAmount()
		{
			Amount = UserInventory.GetChestKeyFragmentAmount(_fragmentType);
		}
	}
}